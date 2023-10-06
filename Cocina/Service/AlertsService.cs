using Cocina.Interfas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocina.Service
{
    public class AlertsService : AlertsInterface
    {
        public async Task AlertSimple(string title, string text)
        {
            await Application.Current.MainPage.DisplayAlert(title,text,"ok");
        }

        public async Task<bool> AlertYesOrNot(string title, string text, string option1, string option2)
        {
            bool response =  await Application.Current.MainPage.DisplayAlert(title, text, option1,option2);
            return response;
        }
    }
}
