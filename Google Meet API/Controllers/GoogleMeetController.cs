using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Google_Meet_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GoogleMeetController : ControllerBase
{
    private CalendarService _calendarService;

    [HttpPost("create-meeting")]
    public async Task<IActionResult> CreateGoogleMeetEvent([FromBody] GoogleMeetRequest request)
    {
        try
        {
            // Initialize the calendar service with the provided ServiceAccountEmail and ApplicationName
            _calendarService = GoogleCalendarService.GetCalendarService(request.ServiceAccountEmail, request.ApplicationName);

            // Map attendees from the request
            var eventAttendees = request.Attendees.Select(a => new EventAttendee
            {
                Email = a.Email,
                DisplayName = a.Name  // Add name to the attendee
            }).ToList();



            request.StartTime = DateTime.Now;
            request.EndTime = request.StartTime.AddMinutes(30);


            // Define event
            Event newEvent = new Event()
            {
                Summary = request.Summary,
                Location = request.Location,
                Description = request.Description,
                Start = new EventDateTime()
                {
                    DateTime = request.StartTime,
                    TimeZone = "Asia/Kolkata",  // Adjust the timezone if needed
                },
                End = new EventDateTime()
                {
                    DateTime = request.EndTime,
                    TimeZone = "Asia/Kolkata",
                },
                Attendees = eventAttendees,  // Add attendees with names and emails
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey
                        {
                            Type = "hangoutsMeet"
                        }
                    }
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                        {
                            new EventReminder { Method = "email", Minutes = 24 * 60 },
                            new EventReminder { Method = "sms", Minutes = 10 },
                        }
                }
            };

            // Add the event to the primary calendar
            EventsResource.InsertRequest insertRequest = _calendarService.Events.Insert(newEvent, "primary");
            insertRequest.ConferenceDataVersion = 1;
            Event createdEvent = await insertRequest.ExecuteAsync();

            // Return the Google Meet link
            var meetLink = createdEvent.ConferenceData.EntryPoints?.FirstOrDefault()?.Uri;
            return Ok(new { MeetLink = meetLink });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}


public class GoogleCalendarService
{
    public static CalendarService GetCalendarService(string ServiceAccountEmail, string ApplicationName)
    {

        string[] scopes = { CalendarService.Scope.Calendar };
        GoogleCredential credential;

        // Load the service account key JSON file
        using (var stream = new FileStream("D:\\Projects\\DotNet\\dotnet8\\api\\Google Meet API\\Google Meet API\\serviceAccountKey.json",
            FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(scopes).CreateWithUser(ServiceAccountEmail);
        }

        // Create the Calendar service
        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        return service;
    }
}

public class GoogleMeetRequest
{
    public string Summary { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<Attendee> Attendees { get; set; }
    public string ServiceAccountEmail { get; set; }
    public string ApplicationName { get; set; }
}

public class Attendee
{
    public string Email { get; set; }
    public string Name { get; set; }
}









//https://localhost:7111/api/GoogleMeet/create-meeting

//{
//  "summary": "Project Test",
//  "location": "Bhubaneswar",
//  "description": "Discussion about the project.",
//  "startTime": "2024-10-25T03:00:31.321Z",
//  "endTime": "2024-10-25T03:30:31.321Z",
//  "attendees": [
//    {
//      "email": "amarjyotimahanta6@gmail.com",
//      "name": "Amarjyoti Mahanta"
//    },
//    {
//    "email": "chittaranjan.das@tatwa.info",
//      "name": "Chitta Ranjan Das"
//    },
//    {
//    "email": "subhadipta.nayak@tatwa.info",
//      "name": "Subhadipta Nayak"
//    },
//     {
//    "email": "ranjita.nayak@tatwa.info",
//      "name": "Ranjita Nayak"
//    },
//    {
//    "email": "ishrita.das@tatwa.info",
//      "name": "Ishrita Das"
//    }
//  ],
//  "serviceAccountEmail": "amarjyoti.mahanta@tatwa.info",
//  "applicationName": "Doctor Consultation"
//}
