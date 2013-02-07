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
    /**
     * This filter uses time to determine if the wiimote actually is in motion
     * or not. This filter only works together with the IdleStateFilter.
     *
     * @author Benjamin 'BePo' Poppinga
     */
    public class MotionDetectFilter : Filter
    {
        private bool nowinmotion;
        private long motionstartstamp;

        /**
         * Defines the time the wiimote has to be in idle state before a new motion change
         * event appears. The default value 500ms should work well, only change it if you are sure
         * about what you're doing.
         * @param time Time in ms
         */
        public int MotionChangeTime { get; set; }

        /***
         * Detects wheather the wiimote receives acceleration or not and
         * raises an event, if the device starts or stops. This is actual a
         * null filter, not manipulating anything. But looks pretty good in
         * this datatype since it could be removed easily.
         * 
         * @param wiimote The Wiimote object which is controlled by the filter.
         */
        public MotionDetectFilter()
        {
            this.reset();
        }

        public override double[] filterAlgorithm(double[] vector)
        {
            if (vector != null)
            {
                this.motionstartstamp = DateTime.Now.Ticks;
                if (!this.nowinmotion)
                {
                    this.nowinmotion = true;
                    this.motionstartstamp = DateTime.Now.Ticks;
                }
            }

            return vector;
        }

        public override double[] filter(double[] vector)
        {

            if (this.nowinmotion &&
                (DateTime.Now.Ticks - this.motionstartstamp) >= this.MotionChangeTime)
            {
                this.nowinmotion = false;
            }

            return filterAlgorithm(vector);
        }

        public override void reset()
        {
            this.motionstartstamp = DateTime.Now.Ticks;
            this.nowinmotion = false;
            this.MotionChangeTime = 190;
        }
    }
}
