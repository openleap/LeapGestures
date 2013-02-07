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
    /**
 * This is a Hidden Markov Model implementation which internally provides
 * the basic algorithms for training and recognition (forward and backward
 * algorithm). Since a regular Hidden Markov Model doesn't provide a possibility
 * to train multiple sequences, this implementation has been optimized for this
 * purposes using some state-of-the-art technologies described in several papers.
 *
 * @author Benjamin 'BePo' Poppinga
 *
 */

    public class HiddenMarkovModel
    {
        /** The number of states */
        protected int numStates;

        /** The number of observations */
        protected int numObservations;

        /** The initial probabilities for each state: p[state] */
        public double[] InitialStateProbabilities { get; set; }

        /** The state change probability to switch from state A to
         * state B: a[stateA][stateB] */
        public double[,] TransitionProbabilities { get; set; }

        /** The probability to emit symbol S in state A: b[stateA][symbolS] */
        public double[,] EmissionProbabilities { get; set; }

        /**
         * Initialize the Hidden Markov Model in a left-to-right version.
         * 
         * @param numStates Number of states
         * @param numObservations Number of observations
         */
        public HiddenMarkovModel(int numStates, int numObservations)
        {
            this.numStates = numStates;
            this.numObservations = numObservations;
            InitialStateProbabilities = new double[numStates];
            TransitionProbabilities = new double[numStates, numStates];
            EmissionProbabilities = new double[numStates, numObservations];
            this.reset();
        }

        /**
         * Reset the Hidden Markov Model to the initial left-to-right values.
         *
         */
        private void reset()
        {
            int jumplimit = 2;

            // set startup probability
            InitialStateProbabilities[0] = 1;
            for (int i = 1; i < numStates; i++)
            {
                InitialStateProbabilities[i] = 0;
            }

            // set state change probabilities in the left-to-right version
            // NOTE: i now that this is dirty and very static. :)
            for (int i = 0; i < numStates; i++)
            {
                for (int j = 0; j < numStates; j++)
                {
                    if (i == numStates - 1 && j == numStates - 1)
                    { // last row
                        TransitionProbabilities[i, j] = 1.0;
                    }
                    else if (i == numStates - 2 && j == numStates - 2)
                    { // next to last row
                        TransitionProbabilities[i, j] = 0.5;
                    }
                    else if (i == numStates - 2 && j == numStates - 1)
                    { // next to last row
                        TransitionProbabilities[i, j] = 0.5;
                    }
                    else if (i <= j && i > j - jumplimit - 1)
                    {
                        TransitionProbabilities[i, j] = 1.0 / (jumplimit + 1);
                    }
                    else
                    {
                        TransitionProbabilities[i, j] = 0.0;
                    }
                }
            }

            // emission probability
            for (int i = 0; i < numStates; i++)
            {
                for (int j = 0; j < numObservations; j++)
                {
                    EmissionProbabilities[i, j] = 1.0 / (double)numObservations;
                }
            }
        }

        /**
         * Trains the Hidden Markov Model with multiple sequences.
         * This method is normally not known to basic hidden markov
         * models, because they usually use the Baum-Welch-Algorithm.
         * This method is NOT the traditional Baum-Welch-Algorithm.
         * 
         * If you want to know in detail how it works please consider
         * my Individuelles Projekt paper on the wiigee Homepage. Also
         * there exist some english literature on the world wide web.
         * Try to search for some papers by Rabiner or have a look at
         * Vesa-Matti Mäntylä - "Discrete Hidden Markov Models with
         * application to isolated user-dependent hand gesture recognition". 
         * 
         */
        public void train(IEnumerable<int[]> trainsequence)
        {
            double[,] a_new = new double[numStates, numStates];
            double[,] b_new = new double[numStates, numObservations];
            // re calculate state change probability a
            for (int i = 0; i < numStates; i++)
            {
                for (int j = 0; j < numStates; j++)
                {
                    double numerator = 0;
                    double denominator = 0;

                    foreach (int[] sequence in trainsequence)
                    {
                        double[,] fwd = this.forwardProc(sequence);
                        double[,] bwd = this.backwardProc(sequence);
                        double prob = this.getProbability(sequence);

                        double numerator_innersum = 0;
                        double denominator_innersum = 0;


                        for (int t = 0; t < sequence.Length - 1; t++)
                        {
                            numerator_innersum += fwd[i, t] * TransitionProbabilities[i, j] * EmissionProbabilities[j, sequence[t + 1]] * bwd[j, t + 1];
                            denominator_innersum += fwd[i, t] * bwd[i, t];
                        }
                        numerator += (1 / prob) * numerator_innersum;
                        denominator += (1 / prob) * denominator_innersum;
                    } // k

                    a_new[i, j] = numerator / denominator;
                } // j
            } // i

            // re calculate emission probability b
            for (int i = 0; i < numStates; i++)
            { // zustaende
                for (int j = 0; j < numObservations; j++)
                {	// symbole
                    double numerator = 0;
                    double denominator = 0;

                    foreach (int[] sequence in trainsequence)
                    {
                        double[,] fwd = this.forwardProc(sequence);
                        double[,] bwd = this.backwardProc(sequence);
                        double prob = this.getProbability(sequence);

                        double numerator_innersum = 0;
                        double denominator_innersum = 0;


                        for (int t = 0; t < sequence.Length - 1; t++)
                        {
                            if (sequence[t] == j)
                            {
                                numerator_innersum += fwd[i, t] * bwd[i, t];
                            }
                            denominator_innersum += fwd[i, t] * bwd[i, t];
                        }
                        numerator += (1 / prob) * numerator_innersum;
                        denominator += (1 / prob) * denominator_innersum;
                    } // k

                    b_new[i, j] = numerator / denominator;
                } // j
            } // i

            this.TransitionProbabilities = a_new;
            this.EmissionProbabilities = b_new;
        }

        /**
         * Traditional Forward Algorithm.
         * 
         * @param o the observationsequence O
         * @return Array[State][Time] 
         * 
         */
        protected double[,] forwardProc(int[] o)
        {
            double[,] f = new double[numStates, o.Length];
            for (int l = 0; l < numStates; l++)
            {
                f[l, 0] = InitialStateProbabilities[l] * EmissionProbabilities[l, o[0]];
            }
            for (int i = 1; i < o.Length; i++)
            {
                for (int k = 0; k < numStates; k++)
                {
                    double sum = 0;
                    for (int l = 0; l < numStates; l++)
                    {
                        sum += f[l, i - 1] * TransitionProbabilities[l, k];
                    }
                    f[k, i] = sum * EmissionProbabilities[k, o[i]];
                }
            }
            return f;
        }

        /**
         * Returns the probability that a observation sequence O belongs
         * to this Hidden Markov Model without using the bayes classifier.
         * Internally the well known forward algorithm is used.
         * 
         * @param o observation sequence
         * @return probability that sequence o belongs to this hmm
         */
        public double getProbability(int[] o)
        {
            double prob = 0.0;
            double[,] forward = this.forwardProc(o);
            //	add probabilities
            for (int i = 0; i < forward.GetLength(0); i++)
            { // for every state
                prob += forward[i, forward.GetLength(1) - 1];
            }
            return prob;
        }


        /**
         * Backward algorithm.
         * 
         * @param o observation sequence o
         * @return Array[State][Time]
         */
        protected double[,] backwardProc(int[] o)
        {
            int T = o.Length;
            double[,] bwd = new double[numStates, T];
            /* Basisfall */
            for (int i = 0; i < numStates; i++)
                bwd[i, T - 1] = 1;
            /* Induktion */
            for (int t = T - 2; t >= 0; t--)
            {
                for (int i = 0; i < numStates; i++)
                {
                    bwd[i, t] = 0;
                    for (int j = 0; j < numStates; j++)
                        bwd[i, t] += (bwd[j, t + 1] * TransitionProbabilities[i, j] * EmissionProbabilities[j, o[t + 1]]);
                }
            }
            return bwd;
        }
    }
}
