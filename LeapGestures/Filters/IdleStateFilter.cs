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
    public class IdleStateFilter : Filter
    {
        /**
     * Defines the absolute value when the system should react to data.
     * This is a parameter for the first of the two filters: idle state
     * filter. The default value 0.1
     * should work well. Only change if you are sure what you're doing.
     * 
     * @param sensivity
     * 		data values smaller than this value wouldn't be detected.
     */
        public double Sensitivity { get; set; }

        /**
         * Since an acceleration sensor usually provides information even
         * if it doesn't move, this filter removes the data if it's in the
         * idle state.
         */
        public IdleStateFilter()
        {
            this.Sensitivity = 0.1;
        }

        public override double[] filterAlgorithm(double[] vector)
        {
            // calculate values needed for filtering:
            // absolute value
            double absvalue = Math.Sqrt((vector[0] * vector[0]) +
                    (vector[1] * vector[1]) + (vector[2] * vector[2]));

            // filter formulaes and return values
            if (absvalue > this.Sensitivity)
            {
                return vector;
            }
            else
            {
                return null;
            }
        }

        public override void reset()
        {
            
        }
    }
}
