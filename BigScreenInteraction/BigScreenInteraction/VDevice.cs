using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
//using WpfApplication1.Observer;

namespace BigScreenInteraction
{
    public class VDevice
    {
        private ArrayList ObserverList = new ArrayList();

        public void NotifyObservers()
        {
            foreach (IObserver observer in ObserverList)
            {
                observer.Update(this);
            }
        }

        public void RegisterObserver(IObserver observer)
        {
            ObserverList.Add(observer);
        }
    }
}
