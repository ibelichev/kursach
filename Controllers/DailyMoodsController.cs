using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodTracker.Data;
using MoodTracker.Models;
using MoodTracker.Services;


namespace MoodTracker.Controllers
{
    public class MoodsController : Controller
    {
        private readonly MoodService _moodService;

        public MoodsController(MoodTrackerContext context)
        {
            _moodService = new MoodService(context);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _moodService.GetAllMoods());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mood = await _moodService.GetUntrackedMoodWithId(id.GetValueOrDefault());
            if (mood == null)
            {
                return NotFound();
            }

            return View(mood);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Color")] Mood mood)
        {
            if (ModelState.IsValid)
            {
                _moodService.AddMood(mood);
                await _moodService.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mood);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mood = await _moodService.GetTrackedMoodWithId(id.GetValueOrDefault());
            if (mood == null)
            {
                return NotFound();
            }
            return View(mood);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Color")] Mood mood)
        {
            if (id != mood.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _moodService.UpdateMood(mood);
                    await _moodService.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_moodService.MoodExists(mood.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mood);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mood = await _moodService.GetUntrackedMoodWithId(id.GetValueOrDefault());
            if (mood == null)
            {
                return NotFound();
            }

            return View(mood);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mood = await _moodService.GetTrackedMoodWithId(id);
            _moodService.RemoveMood(mood);

            await _moodService.DeleteDailyMoodsWithMood(mood.Id);

            await _moodService.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}