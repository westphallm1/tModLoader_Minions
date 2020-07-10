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
    public class CircularVectorQueue
    {
        protected int queueSize;
        protected float[] backingArray;
        protected int startingPosition;
        protected readonly int headerSize;
        public int Length = 0;
        private int headPosition
        {
            get { return (int)backingArray[startingPosition]; }
            set
            {
                backingArray[startingPosition] = value;
            }
        }

        private int tailPosition
        {
            get { return (int)backingArray[startingPosition + 1]; }
            set { backingArray[startingPosition+1] = value; }
        }

        private int endOfQueue
        {
            get { return startOfQueue + 2 * queueSize; }
        }

        private int startOfQueue
        {
            get { return startingPosition + headerSize; }
        }

        private int NextHeadPosition()
        {
            if (headPosition + 2 < endOfQueue)
            {
                return headPosition + 2;
            } else
            {
                return startOfQueue;
            }
        }
        private int PreviousHeadPosition()
        {
            if (headPosition - 2 >= startOfQueue)
            {
                return headPosition - 2;
            } else
            {
                return endOfQueue - 2;
            }
        }

        public Vector2 SeekBackwards(int index)
        {
            int headIndex;
            if(headPosition - 2 * index >= startOfQueue)
            {
                headIndex = headPosition - 2 * index;
            } else
            {
                int distancePastStart = startOfQueue - (headPosition - 2 * index);
                headIndex = endOfQueue + distancePastStart;
            }
            return new Vector2(backingArray[headIndex], backingArray[headIndex + 1]);
        }

        private int NextTailPosition()
        {
            if (tailPosition + 2 < endOfQueue)
            {
                return tailPosition + 2;
            } else
            {
                return startOfQueue;
            }
        }
        public void Enqueue(Vector2 position)
        {
            if(IsFull())
            {
                throw new IndexOutOfRangeException("Queue is full!");
            }
            Length += 1;
            backingArray[headPosition] = position.X;
            backingArray[headPosition + 1] = position.Y;
            headPosition = NextHeadPosition();
        }

        public Vector2 Peek()
        {
            if(IsEmpty())
            {
                throw new IndexOutOfRangeException("Queue is full!");
            }
            int prevHead = PreviousHeadPosition();
            return new Vector2(backingArray[prevHead], backingArray[prevHead + 1]);
        }
        public void PeekWrite(Vector2 toWrite)
        {
            if(IsEmpty())
            {
                throw new IndexOutOfRangeException("Queue is full!");
            }
            int prevHead = PreviousHeadPosition();
            backingArray[prevHead] = toWrite.X;
            backingArray[prevHead + 1] = toWrite.Y;
        }

        public Vector2 Dequeue()
        {
            if(IsEmpty())
            {
                throw new IndexOutOfRangeException("Queue is full!");
            }
            Length -= 1;
            Vector2 dequeued = new Vector2(backingArray[tailPosition], backingArray[tailPosition + 1]);
            tailPosition = NextTailPosition();
            return dequeued;

        }

        public bool IsFull ()
        {
            return NextHeadPosition() == tailPosition;
        }

        public bool IsEmpty ()
        {
            return headPosition == tailPosition;
        }

        public CircularVectorQueue(float[] backing, int startingPosition = 0, int headerSize = 2, int queueSize = 16)
        {
            backingArray = backing ?? new float[queueSize];
            this.startingPosition = startingPosition;
            if(startingPosition + 2 * queueSize > backing.Length)
            {
                throw new IndexOutOfRangeException("Backing array not sufficient to hold queue!");
            }
            this.queueSize = queueSize;
            this.headerSize = headerSize;
        }

        public static void Initialize(float[] backing, int startingPosition = 0, int headerSize = 2)
        {
            var queue = new CircularVectorQueue(backing, startingPosition, headerSize);
            queue.headPosition = queue.startOfQueue;
            queue.tailPosition = queue.startOfQueue;
        }
    }
}
