using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenSundayApi.Models;

namespace OpenSundayApi.Controllers
{
  #region LocationsController
  [Route("api/[controller]")]
  [ApiController]
  public class LocationsController : ControllerBase
  {
    private readonly OpenSundayContext _context;

    public LocationsController(OpenSundayContext context)
    {
      _context = context;
    }
    #endregion

    // GET: api/Locations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
      //get all locations from the database
      return await _context.Location.ToListAsync();
    }

    #region snippet_GetByID
    // GET: api/Locations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocation(int id)
    {
      //get a location by it ID
      var location = await _context.Location.FindAsync(id);

      if (location == null)
      {
        
        return NotFound();
      }

      return location;
    }
    #endregion

    #region snippet_Update
    // PUT: api/Location/5
    [HttpPut]
    public async Task<IActionResult> PutLocation(LocationCityCat l)
    {
      var user_id = User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
      
  
     var locations = await _context.Location.Where(loc => (loc.Id == (l.Id))).ToListAsync();   


     Location location = new Location();
      if (locations != null)
      {
        foreach(var lc in locations){
            location = lc;
        }
      }

      location.Name = l.Name;
      // Add creator ID based on the Auth0 User ID found in the JWT token
      location.Creator = User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
      location.Telephone = l.Telephone;
      location.OpeningTime =l.OpeningTime;
      location.ClosingTime = l.ClosingTime;

       if (l.Id != location.Id)
      {
        return BadRequest();
      }

      _context.Entry(location).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!LocationExists(l.Id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }
    #endregion


    #region snippet_Create
    // POST: api/Locations
    [HttpPost]
    public async Task<ActionResult<Location>> PostLocation(LocationCityCat l)
    {
      //check if user is already in db 
      var user_id = User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
      
      var users = await _context.User.Where(u => (u.Id == (user_id))).ToListAsync();
      //add user to db
      if(!(users.Count >0)){
                User u = new User();
        u.Id = user_id;
        _context.User.Add(u);
        await _context.SaveChangesAsync();
      }
  
      //get City with the NPA of the post location from DB
      var city = await _context.City.FindAsync(l.NPA);

      //If city does not exist in DB we add it
      if(city == null){
      City c = new City();
      c.NPA = l.NPA;
      c.Name = l.CityName;

        _context.City.Add(c);
        await _context.SaveChangesAsync();
      }

      //As in our post the categoryname is a string we have to retrieve its id from the DB
      var categories = await _context.Category.Where(cat => (cat.Name == (l.CategoryName))).ToListAsync();
      int catId = 0;

      foreach(var cat in categories){
        catId = cat.Id;
      }

      //Create a location Object according to its attributes in the DB
      Location location = new Location();
      location.Id = l.Id;
      location.Name = l.Name;
      // Add creator ID based on the Auth0 User ID found in the JWT token
      location.Creator = User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
      location.Latitude = l.Latitude;
      location.Longitude = l.Longitude;
      location.Address = l.Address;
      location.Telephone = l.Telephone;
      location.OpeningTime =l.OpeningTime;
      location.ClosingTime = l.ClosingTime;
      location.FK_Category = catId;
      location.FK_City = l.NPA;

      //check if location exists if not do not add (by lat, long and name)
      var locations = await _context.Location.Where(loc => (loc.Name == location.Name)).ToListAsync();
  
      if (locations != null)
      {
        foreach(var lc in locations){
          if((lc.Longitude == location.Longitude) && (lc.Latitude == location.Latitude)){
            return null; //if exists return null
          }
        }
      }

      //if location does not exists we add it to the db
      _context.Location.Add(location);
      await _context.SaveChangesAsync();

    //return of the inserted object from the db
      return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
    }
    #endregion

    #region snippet_Delete
    // DELETE: api/Locations/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<Location>> DeleteLocation(int id)
    {

      //remove location
      var location = await _context.Location.FindAsync(id);
      if (location == null)
      {
        return NotFound();
      }

      _context.Location.Remove(location);
      await _context.SaveChangesAsync();

      return location;
    }
    #endregion

    private bool LocationExists(int id)
    {
      return _context.Location.Any(e => e.Id == id);
    }
  }
}