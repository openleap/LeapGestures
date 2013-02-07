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
    public class TriggeredProcessingUnit : ProcessingUnit
    {
        // gesturespecific values
        private Gesture current; // current gesture

        private List<Gesture> trainsequence;

        // State variables
        private bool learning, analyzing;

        public TriggeredProcessingUnit(bool autofilter)
            : base(autofilter)
        {
            this.learning = false;
            this.analyzing = false;
            this.current = new Gesture();
            this.trainsequence = new List<Gesture>();
        }

        protected override void AddAcceleration(AccelerationVector acceleration)
        {
            if (this.learning || this.analyzing)
            {
                this.current.add(acceleration); // add event to gesture
            }
        }

        public void StartTraining()
        {
            if (!this.analyzing && !this.learning)
            {
                this.learning = true;
            }
        }

        public GestureModel FinishTrainingSession()
        {
            if (!this.analyzing && !this.learning)
            {
                if (this.trainsequence.Any())
                {
                    // Training the model with this.trainsequence.Count gestures...
                    this.learning = true;

                    GestureModel m = new GestureModel();
                    m.Train(this.trainsequence);
                    this.classifier.addGestureModel(m);

                    this.trainsequence = new List<Gesture>();
                    this.learning = false;

                    return m;
                }
            }

            return null;
        }

        public void StartRecognition()
        {
            if (!this.analyzing && !this.learning)
            {
                this.analyzing = true;
            }
        }

        public void StopTraining()
        {
            if (this.learning)
            {
                if (this.current.getCountOfData() > 0)
                {
                    Gesture gesture = new Gesture(this.current);
                    this.trainsequence.Add(gesture);
                    this.current = new Gesture();
                }

                this.learning = false;
            }
        }

        public void StopRecognition()
        {
            if (this.analyzing)
            {
                if (this.current.getCountOfData() > 0)
                {
                    Gesture gesture = new Gesture(this.current);

                    var recognized = this.classifier.classifyGesture(gesture);
                    if (recognized != null)
                    {
                        double recogprob = this.classifier.getLastProbability();
                        this.OnGestureRecognized(recognized, recogprob);
                    }
                    else
                    {
                        this.OnGestureRecognized(null, 0.0);
                        // No gesture recognized
                    }

                    this.current = new Gesture();
                }

                this.analyzing = false;
            }
        }
    }
}
