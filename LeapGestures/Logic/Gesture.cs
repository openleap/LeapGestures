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
    public class Gesture : ICloneable
    {
        /** Min/MaxAcceleration setup manually? */
        private Boolean minmaxmanual;
        private double minacc;
        private double maxacc;

        /** The complete trajectory as WiimoteAccelerationEvents
         * as a vector. It's a vector because we don't want to
         * loose the chronology of the stored events.
         */
        private List<AccelerationVector> data;

        /**
         * Create an empty Gesture.
         */
        public Gesture()
        {
            this.data = new List<AccelerationVector>();
        }

        /** 
         * Make a deep copy of another Gesture object.
         * 
         * @param original Another Gesture object
         */
        public Gesture(Gesture original)
        {
            this.data = new List<AccelerationVector>();
            var origin = original.getData();
            for (int i = 0; i < origin.Count; i++)
            {
                this.add((AccelerationVector)origin[i]);
            }
        }

        public object Clone()
        {
            return new Gesture(this);
        }

        /**
         * Adds a new acceleration event to this gesture.
         * 
         * @param event The WiimoteAccelerationEvent to add.
         */
        public void add(AccelerationVector acceleration)
        {
            this.data.Add(acceleration);
        }

        /**
         * Returns the last acceleration added to this gesture.
         * 
         * @return the last acceleration event added.
         */
        public AccelerationVector getLastData()
        {
            return (AccelerationVector)this.data[this.data.Count - 1];
        }

        /**
         * Returns the whole chronological sequence of accelerations as
         * a vector.
         * 
         * @return chronological sequence of accelerations.
         */
        public List<AccelerationVector> getData()
        {
            return this.data;
        }

        /**
         * Removes the first element of the acceleration queue of a gesture
         */
        public void removeFirstData()
        {
            this.data.RemoveAt(0);
        }

        public int getCountOfData()
        {
            return this.data.Count;
        }

        public void setMaxAndMinAcceleration(double max, double min)
        {
            this.maxacc = max;
            this.minacc = min;
            this.minmaxmanual = true;
        }

        public double getMaxAcceleration()
        {
            if (!this.minmaxmanual)
            {
                double maxacc = Double.MinValue;
                for (int i = 0; i < this.data.Count; i++)
                {
                    if (Math.Abs(this.data[i].X) > maxacc)
                    {
                        maxacc = Math.Abs(this.data[i].X);
                    }
                    if (Math.Abs(this.data[i].Y) > maxacc)
                    {
                        maxacc = Math.Abs(this.data[i].Y);
                    }
                    if (Math.Abs(this.data[i].Z) > maxacc)
                    {
                        maxacc = Math.Abs(this.data[i].Z);
                    }
                }
                return maxacc;
            }
            else
            {
                return this.maxacc;
            }
        }

        public double getMinAcceleration()
        {
            if (!this.minmaxmanual)
            {
                double minacc = Double.MaxValue;
                for (int i = 0; i < this.data.Count; i++)
                {
                    if (Math.Abs(this.data[i].X) < minacc)
                    {
                        minacc = Math.Abs(this.data[i].X);
                    }
                    if (Math.Abs(this.data[i].Y) < minacc)
                    {
                        minacc = Math.Abs(this.data[i].Y);
                    }
                    if (Math.Abs(this.data[i].Z) < minacc)
                    {
                        minacc = Math.Abs(this.data[i].Z);
                    }
                }
                return minacc;
            }
            else
            {
                return this.minacc;
            }
        }
    }
}
