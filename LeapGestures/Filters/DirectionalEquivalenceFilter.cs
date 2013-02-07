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
    public class DirectionalEquivalenceFilter : Filter
    {
        private double[] reference;

        public double Sensitivity { get; set; }

        public DirectionalEquivalenceFilter()
        {
            this.reset();
        }

        public override double[] filterAlgorithm(double[] vector)
        {
            if (vector[0] < reference[0] - this.Sensitivity ||
           vector[0] > reference[0] + this.Sensitivity ||
           vector[1] < reference[1] - this.Sensitivity ||
           vector[1] > reference[1] + this.Sensitivity ||
           vector[2] < reference[2] - this.Sensitivity ||
           vector[2] > reference[2] + this.Sensitivity)
            {
                this.reference = vector;
                return vector;
            }
            else
            {
                return null;
            }
        }

        public override void reset()
        {
            this.Sensitivity = 0.2;
            this.reference = new double[] { 0.0, 0.0, 0.0 };
        }
    }
}
