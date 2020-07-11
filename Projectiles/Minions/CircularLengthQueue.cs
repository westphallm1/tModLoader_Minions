using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace DemoMod.Projectiles.Minions
{
    public class CircularLengthQueue : CircularVectorQueue
    {
        private int LengthResolution;
        // gotta watch for precision
        private int MaxSaveDistance;
        public double SavedDistance = 0;

        public void AddPosition(Vector2 position)
        {
            if(Length == 0)
            {
                Enqueue(position);
                return;
            } else if (Length == 1)
            {
                Vector2 lastPos = Peek();
                SavedDistance = Vector2.Distance(lastPos, position);
                Enqueue(position);
                return;
            }
            Vector2 lastSaved = SeekBackwards(2);
            Vector2 currentHead = Peek();
            float distance = Vector2.Distance(lastSaved, position);
            if(distance < LengthResolution)
            {
                // overwrite the current head, since they're both close to the previous one
                SavedDistance -= Vector2.Distance(lastSaved, currentHead);
                SavedDistance += distance;
                PeekWrite(position);
            } else
            {
                SavedDistance += Vector2.Distance(currentHead, position);
                Enqueue(position);
            }

            if(SavedDistance > MaxSaveDistance)
            {
                TrimTail();
            }
        }

        private void TrimTail()
        {
            while(SavedDistance > MaxSaveDistance && Length > 1)
            {
                Vector2 tail = SeekBackwards(Length);
                Vector2 next = SeekBackwards(Length - 1);
                SavedDistance -= Vector2.Distance(tail, next);
                Dequeue();
            }
        }

        public Vector2 PositionAlongPath(float distanceAlongPath, ref Vector2 direction)
        {
            if(distanceAlongPath >= SavedDistance || Length < 2)
            {
                return SeekBackwards(Length);
            }
            float distance = 0;
            Vector2 current = Peek();
            Vector2 next = current;
            for(int i = 2; i <= Length; i++ )
            {
                next = SeekBackwards(i);
                distance += Vector2.Distance(current, next);
                if(distance >= distanceAlongPath)
                {
                    break;
                }
                current = next;
            }
            float overshoot = distance - distanceAlongPath;
            Vector2 overshootDirection = Vector2.Normalize(next - current);
            direction = overshootDirection;
            return next - overshoot * overshootDirection;
        }

        public CircularLengthQueue(float[] backing, int startingPosition = 0, int headerSize = 2, int queueSize = 16, int lengthResolution = 16, int maxLength = 220) :
            base(backing, startingPosition, headerSize, queueSize)
        {
            LengthResolution = lengthResolution;
            MaxSaveDistance = maxLength;
        }
    }
}
