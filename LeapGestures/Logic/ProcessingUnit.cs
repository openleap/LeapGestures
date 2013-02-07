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
using LeapGestures.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapGestures.Logic
{
    public abstract class ProcessingUnit
    {
        // Classifier
        protected Classifier classifier;

        protected List<Filter> dataFilters = new List<Filter>();

        public event EventHandler<GestureEventArgs> GestureReceived;

        public ProcessingUnit(bool autofilter = false)
        {
            this.classifier = new Classifier();

            if (autofilter)
            {
                this.AddFilter(new IdleStateFilter());
                this.AddFilter(new MotionDetectFilter());
                this.AddFilter(new DirectionalEquivalenceFilter());
            }
        }

        public void AddFilter(Filter filter)
        {
            this.dataFilters.Add(filter);
        }

        public void ResetFilters()
        {
            foreach (var filter in this.dataFilters)
            {
                filter.reset();
            }
        }

        public void ClearFilters()
        {
            this.dataFilters.Clear();
        }

        protected void OnGestureRecognized(GestureModel gesture, double probability)
        {
            if (GestureReceived != null)
            {
                GestureEventArgs w = new GestureEventArgs(gesture, probability);
                GestureReceived(this, w);
            }
        }

        public void AddData(double[] vector)
        {
            foreach (var filter in this.dataFilters)
            {
                vector = filter.filter(vector);
            }

            // don't need to create an event if filtered away
            if (vector != null)
            {
                // 	calculate the absolute value for the accelerationevent
                double absvalue = Math.Sqrt((vector[0] * vector[0]) +
                        (vector[1] * vector[1]) + (vector[2] * vector[2]));

                AccelerationVector w = new AccelerationVector(vector[0], vector[1], vector[2]);
                AddAcceleration(w);
            }
        }

        protected abstract void AddAcceleration(AccelerationVector acceleration);

        /**
         * Resets the complete gesturemodel. After reset no gesture is known
         * to the system.
         */
        public void reset()
        {
            if (this.classifier.getCountOfGestures() > 0)
            {
                this.classifier.clear();
            }
        }
    }
}
