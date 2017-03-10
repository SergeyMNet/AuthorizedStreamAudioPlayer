using System.IO;
using System.Threading.Tasks;
using DotNetCoreApp.Filters;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetCoreApp.Controllers
{
    [HmacAuthorize]
    [Route("api/v1/[controller]")]
    public class DataController : Controller
    {
        /// <summary>
        /// GET http://myserver/api/v1/data/sound
        /// Get mp3 file
        /// 
        /// todo: you may add and use Guid for search and get files
        /// Use DirectorySeparatorChar for different operating systems
        /// -- GetSound(Guid id)
        /// -- string filePath = Path.Combine(System.AppContext.BaseDirectory,
        /// --  "Data{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}{id}{Path.DirectorySeparatorChar}Sound{Path.DirectorySeparatorChar}sample.mp3");
        /// 
        /// </summary>
        /// <returns>mp3</returns>
        [HttpGet("sound")]
        public async Task<IActionResult> GetSound()
        {
            // todo use your path to file
            string filePath = Path.Combine(System.AppContext.BaseDirectory,
             $"Data{Path.DirectorySeparatorChar}Sound{Path.DirectorySeparatorChar}sample.mp3");

             var bytes = new byte[0];
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var br = new BinaryReader(fs);
                long numBytes = new FileInfo(filePath).Length;
                bytes = br.ReadBytes((int)numBytes);
                
                return new FileContentResult(bytes, "audio/mpeg");
            }
        }
    }
}
