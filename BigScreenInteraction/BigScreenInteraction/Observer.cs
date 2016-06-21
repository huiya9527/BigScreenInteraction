using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using VDevice;
namespace BigScreenInteraction
{
    public interface IObserver
    {
        void Update(VDevice dev);
        void Update();
    }
}
