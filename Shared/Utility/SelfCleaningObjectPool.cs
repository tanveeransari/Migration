using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utility
{
    public class SelfCleaningObjectPool<T> : IDisposable
        where T : ICleanMyself
    {
        private int minSize = 10000;
        private Func<T> _objectGenerator;
        private Queue<T> _objects;
        private object lockObj = new object();
        private int count;

        public SelfCleaningObjectPool(Func<T> objectGenerator,int pMinSize)
        {
            this.minSize = pMinSize;
            _objects = new Queue<T>(this.minSize);
            _objectGenerator = objectGenerator;

            for (int i = 0; i < this.minSize; i++)
            {
                _objects.Enqueue(_objectGenerator());
            }
        }

        public T GetObject()
        {
            lock (this.lockObj)
            {
                if (_objects.Count > 0)
                    return _objects.Dequeue();

                count++;
                return _objectGenerator();
            }
        }

        public void PutObject(T item)
        {
            lock (this.lockObj)
            {
                if (count == 0)
                {
                    item.Cleanup();
                    _objects.Enqueue(item);
                }
                else
                {
                    using (item as IDisposable)
                    {
                        count--;
                    }
                }
            }
        }


        ~SelfCleaningObjectPool()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                lock (this.lockObj)
                {
                    while (_objects.Count > 0)
                    {
                        using (_objects.Dequeue() as IDisposable)
                        {
                        }
                    }
                }
            }
        }

    }
}
