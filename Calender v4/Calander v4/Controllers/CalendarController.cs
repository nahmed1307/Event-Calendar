using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using DHTMLX.Scheduler; //Nuget packages for the Calender 
using DHTMLX.Common;
using DHTMLX.Scheduler.Data;
using DHTMLX.Scheduler.Controls;

using Calander_v4.Models;
namespace Calander_v4.Controllers
{
    public class CalendarController : Controller
    {
        public ActionResult Index()
        {
            //Being initialized in that way, scheduler will use CalendarController.Data as a the datasource and CalendarController.Save to process changes
            var scheduler = new DHXScheduler(this);

            /*
             * It's possible to use different actions of the current controller
             *      var scheduler = new DHXScheduler(this);     
             *      scheduler.DataAction = "ActionName1";
             *      scheduler.SaveAction = "ActionName2";
             * 
             * Or to specify full paths
             *      var scheduler = new DHXScheduler();
             *      scheduler.DataAction = Url.Action("Data", "Calendar");
             *      scheduler.SaveAction = Url.Action("Save", "Calendar");
             */

            /*
             * The default codebase folder is ~/Scripts/dhtmlxScheduler. It can be overriden:
             *      scheduler.Codebase = Url.Content("~/customCodebaseFolder");
             */
            
 
           

            scheduler.LoadData = true;
            scheduler.EnableDataprocessor = true;

            return View(scheduler);
        }

        public ContentResult Data()
        {
            var data = new SchedulerAjaxData(new DataClasses1DataContext() .Events); //Using the Events table from our database

                
            return (ContentResult)data;
        }

        public ContentResult Save(int? id, FormCollection actionValues)
        {
            var action = new DataAction(actionValues);  //Using Data Action to load the data from the database
            
            try
            {
                var changedEvent = (Event)DHXEventsHelper.Bind(typeof(Event), actionValues);
                var data = new DataClasses1DataContext();
     

                switch (action.Type)
                {
                    case DataActionTypes.Insert: //Inserting a new event
                        data.Events.InsertOnSubmit(changedEvent);
                        break;
                    case DataActionTypes.Delete: //deleting an event
                        changedEvent = data.Events.SingleOrDefault(ev => ev.id == action.SourceId);
                            data.Events.DeleteOnSubmit(changedEvent);
                        break;
                    default:// updating an event                          
                        var eventToUpdate = data.Events.SingleOrDefault(ev => ev.id == action.SourceId);
                        DHXEventsHelper.Update(eventToUpdate, changedEvent, new List<string>() { "id" });
                        break;
                }
                data.SubmitChanges();
                action.TargetId = changedEvent.id;


            }
            catch
            {
                action.Type = DataActionTypes.Error;
            }
            return (ContentResult)new AjaxSaveResponse(action);
        }
    }
}

