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
    public class Quantizer
    {
        private const int CLUSTERS = 14;

        /** This is the initial radius of this model. */
        private double radius;

        /** Number of states from the following Hidden Markov Model */
        private int numStates;

        /** The representation of the so called Centeroids */
        private double[][] map;

        /** True, if map is already trained. */
        private bool maptrained;

        /**
         * Initialize a empty quantizer. The states variable is necessary since some
         * algorithms need this value to calculate their values correctly.
         * 
         * @param numStates
         *            number of hidden markov model states
         */
        public Quantizer(int numStates)
        {
            this.numStates = numStates;
            this.map = new double[CLUSTERS][];
            this.maptrained = false;
        }

        /**
         * Trains this Quantizer with a specific gesture. This means that the
         * positions of the centeroids would adapt to this training gesture. In our
         * case this would happen with a summarized virtual gesture, containing all
         * the other gestures.
         * 
         * @param gesture
         *            the summarized virtual gesture
         */
        public void trainCenteroids(Gesture gesture) {
        List<AccelerationVector> data = gesture.getData();
        double pi = Math.PI;
        this.radius = (gesture.getMaxAcceleration() + gesture.getMinAcceleration()) / 2;
        
        // x , z , y
        if (!this.maptrained) {
            this.maptrained = true;
            this.map[0] = new double[] { this.radius, 0.0, 0.0 };
            this.map[1] = new double[] { Math.Cos(pi / 4) * this.radius, 0.0,
                    Math.Sin(pi / 4) * this.radius };
            this.map[2] = new double[] { 0.0, 0.0, this.radius };
            this.map[3] = new double[] { Math.Cos(pi * 3 / 4) * this.radius,
                    0.0, Math.Sin(pi * 3 / 4) * this.radius };
            this.map[4] = new double[] { -this.radius, 0.0, 0.0 };
            this.map[5] = new double[] { Math.Cos(pi * 5 / 4) * this.radius,
                    0.0, Math.Sin(pi * 5 / 4) * this.radius };
            this.map[6] = new double[] { 0.0, 0.0, -this.radius };
            this.map[7] = new double[] { Math.Cos(pi * 7 / 4) * this.radius,
                    0.0, Math.Sin(pi * 7 / 4) * this.radius };

            this.map[8] = new double[] { 0.0, this.radius, 0.0 };
            this.map[9] = new double[] { 0.0, Math.Cos(pi / 4) * this.radius,
                    Math.Sin(pi / 4) * this.radius };
            this.map[10] = new double[] { 0.0,
                    Math.Cos(pi * 3 / 4) * this.radius,
                    Math.Sin(pi * 3 / 4) * this.radius };
            this.map[11] = new double[] { 0.0, -this.radius, 0.0 };
            this.map[12] = new double[] { 0.0,
                    Math.Cos(pi * 5 / 4) * this.radius,
                    Math.Sin(pi * 5 / 4) * this.radius };
            this.map[13] = new double[] { 0.0,
                    Math.Cos(pi * 7 / 4) * this.radius,
                    Math.Sin(pi * 7 / 4) * this.radius };
        }

        int[,] g_alt = new int[this.map.Length, data.Count];
        int[,] g = new int[this.map.Length, data.Count];

        do {
            // Derive new Groups...
            g_alt = this.copyarray(g);
            g = this.deriveGroups(gesture);

            // calculate new centeroids
            for (int i = 0; i < this.map.GetLength(0); i++) {
                double zaehlerX = 0;
                double zaehlerY = 0;
                double zaehlerZ = 0;
                int nenner = 0;
                for (int j = 0; j < data.Count; j++) {
                    if (g[i,j] == 1) {
                        var e = data[j];
                        zaehlerX += e.X;
                        zaehlerY += e.Y;
                        zaehlerZ += e.Z;
                        nenner++;
                    }
                }
                if (nenner > 1) { // nur wenn der nenner>0 oder >1??? ist muss
                                    // was
                    // geaendert werden
                    // Log.write("Setze neuen Centeroid!");
                    this.map[i] = new double[] {(zaehlerX / (double) nenner),
                                                (zaehlerY / (double) nenner),
                                                (zaehlerZ / (double) nenner) };
                    // Log.write("Centeroid: "+i+": "+newcenteroid[0]+":"+newcenteroid[1]);
                }
            } // new centeroids

        } while (!equalarrays(g_alt, g));

        // Debug: Printout groups
        /*
         * for (int i = 0; i < n; i++) { for (int j = 0; j < this.data.Count;
         * j++) { Log.write(g[i][j] + "|"); } Log.write(""); }
         */

    }

        /**
         * This methods looks up a Gesture to a group matrix, used by the
         * k-mean-algorithm (traincenteroid method) above.
         * 
         * @param gesture
         *            the gesture
         */
        private int[,] deriveGroups(Gesture gesture)
        {
            List<AccelerationVector> data = gesture.getData();
            int[,] groups = new int[this.map.Length, data.Count];

            // Calculate cartesian distance
            double[,] d = new double[this.map.Length, data.Count];
            double[] curr = new double[3];
            double[] vector = new double[3];

            for (int i = 0; i < this.map.Length; i++)
            { // lines
                var line = this.map[i];
                for (var j = 0; j < data.Count; j++)
                { // split
                    var accel = data[j];

                    curr[0] = accel.X;
                    curr[1] = accel.Y;
                    curr[2] = accel.Z;

                    vector[0] = line[0] - curr[0];
                    vector[1] = line[1] - curr[1];
                    vector[2] = line[2] - curr[2];
                    d[i,j] = Math.Sqrt((vector[0] * vector[0])
                            + (vector[1] * vector[1]) + (vector[2] * vector[2]));
                }
            }

            // look, to which group a value belongs
            for (int j = 0; j < data.Count; j++)
            {
                double smallest = Double.MaxValue;
                int row = 0;
                for (int i = 0; i < this.map.GetLength(0); i++)
                {
                    if (d[i, j] < smallest)
                    {
                        smallest = d[i, j];
                        row = i;
                    }
                    groups[i, j] = 0;
                }
                groups[row, j] = 1; // group set
            }

            return groups;
        }

        /**
         * With this method you can transform a gesture to a discrete symbol
         * sequence with values between 0 and granularity (number of observations).
         * 
         * @param gesture
         *            Gesture to get the observationsequence to.
         */
        public int[] getObservationSequence(Gesture gesture)
        {
            int[,] groups = this.deriveGroups(gesture);
            List<int> sequence = new List<int>();

            for (int j = 0; j < groups.GetLength(1); j++)
            { // spalten
                for (int i = 0; i < groups.GetLength(0); i++)
                { // zeilen
                    if (groups[i,j] == 1)
                    {
                        sequence.Add(i);
                        break;
                    }
                }
            }

            // this is very dirty! it have to be here because if not
            // too short sequences would cause an error. i've to think about a
            // better resolution than copying the old value a few time.
            while (sequence.Count < this.numStates)
            {
                sequence.Add(sequence[sequence.Count - 1]);
            }

            int[] output = new int[sequence.Count];
            for (int i = 0; i < sequence.Count; i++)
            {
                output[i] = sequence[i];
            }

            return output;
        }

        /**
         * Function to deepcopy an array.
         */
        private int[,] copyarray(int[,] alt)
        {
            int[,] neu = new int[alt.GetLength(0), alt.GetLength(1)];
            for (int i = 0; i < alt.GetLength(0); i++)
            {
                for (int j = 0; j < alt.GetLength(1); j++)
                {
                    neu[i,j] = alt[i,j];
                }
            }
            return neu;
        }

        /**
         * Function to look if the two arrays containing the same values.
         */
        private bool equalarrays(int[,] one, int[,] two)
        {
            return Enumerable.SequenceEqual(one.Cast<int>(), two.Cast<int>());
        }

        public double getRadius()
        {
            return this.radius;
        }

        public double[][] getHashMap()
        {
            return this.map;
        }

        public void setUpManually(double[][] map, double radius)
        {
            this.map = map;
            this.radius = radius;
        }
    }
}
