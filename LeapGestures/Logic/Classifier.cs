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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapGestures.Logic
{
    public class Classifier
    {
        private List<GestureModel> gesturemodel; // each gesturetype got its own 
        // gesturemodel in this List
        private double lastprob;

        public Classifier()
        {
            this.gesturemodel = new List<GestureModel>();
            this.lastprob = 0.0;
        }

        /** 
         * This method recognize a specific gesture, given to the procedure.
         * For classification a bayes classification algorithm is used.
         * 
         * @param g	gesture to classify
         */
        public GestureModel classifyGesture(Gesture g)
        {
            // Calculate the denominator, Bayesian
            double sum = 0;
            foreach (var model in gesturemodel)
            {
                sum += model.DefaultProbability *
                        model.GetMatchProbability(g);
            }

            GestureModel recognized = null; // which gesture has been recognized
            double recogprob = Int32.MinValue; // probability of this gesture
            double probgesture = 0; // temporal value for bayes algorithm
            double probmodel = 0; // temporal value for bayes algorithm
            foreach (var model in gesturemodel)
            {
                double tmpgesture = model.GetMatchProbability(g);
                double tmpmodel = model.DefaultProbability;

                if (((tmpmodel * tmpgesture) / sum) > recogprob)
                {
                    probgesture = tmpgesture;
                    probmodel = tmpmodel;
                    recogprob = ((tmpmodel * tmpgesture) / sum);
                    recognized = model;
                }
            }

            // a gesture could be recognized
            if (recogprob > 0 && probmodel > 0 && probgesture > 0 && sum > 0)
            {
                this.lastprob = recogprob;
                return recognized;
            }
            else
            {
                // no gesture could be recognized
                return null;
            }
        }

        public double getLastProbability()
        {
            return this.lastprob;
        }

        public void addGestureModel(GestureModel gm)
        {
            this.gesturemodel.Add(gm);
        }

        public GestureModel getGestureModel(int id)
        {
            return this.gesturemodel[id];
        }

        public List<GestureModel> getGestureModels()
        {
            return this.gesturemodel;
        }

        public int getCountOfGestures()
        {
            return this.gesturemodel.Count;
        }

        public void clear()
        {
            this.gesturemodel.Clear();
        }
    }
}
