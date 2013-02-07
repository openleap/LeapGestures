/*
 * wiigee - accelerometerbased gesture recognition
 * Copyright (C) 2007, 2008, 2009 Benjamin Poppinga
 * 
 * Developed at University of Oldenburg
 * Contact: wiigee@benjaminpoppinga.de
 * 
 * LeapGestures - derivate work of wiigee for the Leap
 * Copyright 2013 Craig Schwartz
 *
 * This file is part of LeapGestures.
 *
 * LeapGestures is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using LeapGestures.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapGestures.Logic
{
    /** 
     * This Class units a Quantizer-Component and an Model-Component.
     * In this implementation a k-mean-algorithm for quantization and
     * a hidden markov model as instance for the model has been used.
     * 
     * @author Benjamin 'BePo' Poppinga
     */
    public class GestureModel
    {
        /** The number of states the hidden markov model consists of */
        private int numStates;

        /** The number of observations for the hmm and k-mean */
        private int numObservations;

        /** The quantization component */
        private Quantizer quantizer;

        /** The statistical model, hidden markov model */
        private HiddenMarkovModel markovmodell;

        /** The default probability of this gesturemodel,
         * needed for the bayes classifier */
        public double DefaultProbability { get; private set; }

        public string Name { get; set; }


        /** Creates a Unit (Quantizer&Model).
         * 
         * @param id
         *  int representation of a gesture "name"/class.
         */
        public GestureModel()
        {
            this.numStates = 15; // states empirical value
            this.numObservations = 20; // observations empirical value
            this.markovmodell = new HiddenMarkovModel(numStates, numObservations); // init model
            this.quantizer = new Quantizer(numStates); // init quantizer
        }

        /**
         * Trains the model to a set of motion-sequences, representing
         * different evaluations of a gesture
         * 
         * @param trainsequence	a vector of gestures
         */
        public void Train(IEnumerable<Gesture> trainsequence)
        {
            // summarize all vectors from the different gestures in one
            // gesture called sum.
            double maxacc = 0;
            double minacc = 0;
            Gesture sum = new Gesture();

            foreach (var gesture in trainsequence)
            {
                IEnumerable<AccelerationVector> t = gesture.getData();

                // add the max and min acceleration, we later get the average
                maxacc += gesture.getMaxAcceleration();
                minacc += gesture.getMinAcceleration();

                // transfer every single accelerationevent of each gesture to
                // the new gesture sum
                foreach (var accelerationEvent in t)
                {
                    sum.add(accelerationEvent);
                }

            }

            // get the average and set it to the sum gesture
            sum.setMaxAndMinAcceleration(maxacc / trainsequence.Count(), minacc / trainsequence.Count());

            // train the centeroids of the quantizer with this master gesture sum.
            this.quantizer.trainCenteroids(sum);

            // convert gesture vector to a sequence of discrete values
            var seqs = new List<int[]>();
            foreach (var gesture in trainsequence)
            {
                seqs.Add(this.quantizer.getObservationSequence(gesture));
            }

            // train the markov model with this derived discrete sequences
            this.markovmodell.train(seqs);

            // set the default probability for use with the bayes classifier
            this.SetDefaultProbability(trainsequence);
        }

        /** 
         * Returns the probability that a gesture matches to this
         * gesture model.
         * 
         * @param gesture a gesture to test.
         * @return probability that the gesture belongs to this gesture
         * model.
         */
        public double GetMatchProbability(Gesture gesture)
        {
            int[] sequence = quantizer.getObservationSequence(gesture);
            return this.markovmodell.getProbability(sequence);
        }

        /**
         * Since the bayes classifier needs a model probability for
         * each model this has to be set once after training. As model
         * probability the average probability value has been choosen.
         * 
         * TODO: try lowest or highest model probability as alternative
         * 
         * @param defsequence the vector of training sequences.
         */
        private void SetDefaultProbability(IEnumerable<Gesture> defsequence)
        {
            double prob = 0;
            foreach (var gesture in defsequence)
            {
                prob += this.GetMatchProbability(gesture);
            }

            this.DefaultProbability = (prob) / defsequence.Count();
        }

        public void setDefaultProbability(double prob)
        {
            this.DefaultProbability = prob;
        }
    }
}
