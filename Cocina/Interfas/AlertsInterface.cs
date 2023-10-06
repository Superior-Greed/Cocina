using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocina.Interfas
{
    // ya se que deberia ser IAlertsService
    public interface AlertsInterface
    {
        Task AlertSimple(string title, string text);
        Task<bool> AlertYesOrNot(string title, string text, string option1, string option2);
    }
}
