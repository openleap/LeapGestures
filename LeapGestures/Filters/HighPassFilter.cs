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

namespace LeapGestures.Filters
{
    public class HighPassFilter : Filter
    {
        private double factor;
        private double[] prevAcc;

        public HighPassFilter()
        {
            this.factor = 0.1;
            this.reset();
        }

        public HighPassFilter(double factor)
        {
            this.factor = factor;
            this.reset();
        }

        public override double[] filterAlgorithm(double[] vector)
        {
            double[] retVal = new double[3];
            prevAcc[0] = vector[0] * this.factor + this.prevAcc[0] * (1.0 - this.factor);
            prevAcc[1] = vector[1] * this.factor + this.prevAcc[1] * (1.0 - this.factor);
            prevAcc[2] = vector[2] * this.factor + this.prevAcc[2] * (1.0 - this.factor);

            retVal[0] = vector[0] - prevAcc[0];
            retVal[1] = vector[1] - prevAcc[1];
            retVal[2] = vector[2] - prevAcc[2];

            return retVal;
        }

        public override void reset()
        {
            this.prevAcc = new double[] { 0.0, 0.0, 0.0 };
        }
    }
}
