using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MGDataAccessLibrary.Models;
using Newtonsoft.Json;

namespace MGDataAccessLibrary.BussinessLogic
{
    public class TicketsProcessor
    {
        public static async Task<int> CreateEvent(EventCreateItem item)
        {
            var result = await DataAccess.WebAPI.PostItem<EventCreateItem, Models.CreateEventResponseItem>(item, DataAccess.ApiEndPoint.CreateEvent);
            return result.EventID;
        }
    }
}
