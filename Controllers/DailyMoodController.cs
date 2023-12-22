using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoodTracker.Data;
using MoodTracker.Models;
using MoodTracker.Services;
using MoodTracker.ViewModels;

namespace MoodTracker.Controllers
{
    public class DailyMoodController : Controller
    {
        private readonly DailyMoodService _dailyMoodService;
        private readonly MoodService _moodService;

        public DailyMoodController(MoodTrackerContext context)
        {
            _dailyMoodService = new DailyMoodService(context);
            _moodService = new MoodService(context);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyMood = await _dailyMoodService.GetUntrackedDailyMoodWithId(id.GetValueOrDefault());

            if (dailyMood == null)
            {
                return NotFound();
            }

            return View(dailyMood);
        }

        public async Task<IActionResult> LogMoodForToday()
        {
            DailyMood moodForToday = await _dailyMoodService.GetUntrackedDailyMoodWithDate(DateTime.Today);

            if (moodForToday != null)
            {
                return RedirectToAction(nameof(Edit), new { id = moodForToday.Id });
            }
            else
            {
                return RedirectToAction(nameof(Create), new { date = DateTime.Today });
            }
        }

        public async Task<IActionResult> Create(DateTime? date)
        {
            DateTime defaultDate = date ?? DateTime.Today;

            DailyMoodViewModel dailyMoodVM = await GetDailyMoodVMForCreate(defaultDate.Date);

            return View(dailyMoodVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,MoodId,MoodIntensity,Notes")] DailyMoodViewModel vm)
        {
            DailyMood dailyMood = new DailyMood();

            if (ModelState.IsValid)
            {
                dailyMood.Date = vm.Date;
                dailyMood.MoodId = vm.MoodId;
                dailyMood.MoodIntensity = vm.MoodIntensity;
                dailyMood.Notes = vm.Notes;
                dailyMood.InputTimestamp = DateTime.Now;

                _dailyMoodService.AddDailyMood(dailyMood);
                await _dailyMoodService.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "YearInMoods");
            }

            vm.MoodList = new SelectList(await _moodService.GetMoodNameDict());

            return View(vm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DailyMoodViewModel vm = await GetDailyMoodVM(id.GetValueOrDefault());

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,MoodId,MoodIntensity,Notes")] DailyMood dailyMood)
        {
            if (id != dailyMood.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dailyMoodService.UpdateDailyMood(dailyMood);
                    await _dailyMoodService.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_dailyMoodService.DailyMoodExists(dailyMood.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), "YearInMoods");
            }

            DailyMoodViewModel vm = await GetDailyMoodVM(id);
            return View(vm);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyMood = await _dailyMoodService.GetTrackedDailyMoodWithId(id.GetValueOrDefault());

            if (dailyMood == null)
            {
                return NotFound();
            }

            return View(dailyMood);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dailyMood = await _dailyMoodService.GetTrackedDailyMoodWithId(id);
            _dailyMoodService.RemoveDailyMood(dailyMood);
            await _dailyMoodService.SaveChangesAsync();
            return RedirectToAction(nameof(Index), "YearInMoods");
        }

        public async Task<DailyMoodViewModel> GetDailyMoodVM(int id)
        {
            var dailyMood = await _dailyMoodService.GetUntrackedDailyMoodWithId(id);
            if (dailyMood == null)
            {
                return null;
            }

            DailyMoodViewModel dailyMoodVM = new DailyMoodViewModel
            {

                Id = dailyMood.Id,
                Date = dailyMood.Date,
                MoodId = dailyMood.MoodId,
                Notes = dailyMood.Notes,
                MoodIntensity = dailyMood.MoodIntensity,
                MoodList = await GetMoodSelectList(dailyMood.MoodId)
        };
            return dailyMoodVM;
        }

        public async Task<DailyMoodViewModel> GetDailyMoodVMForCreate(DateTime date)
        {
            DailyMoodViewModel dailyMoodVM = new DailyMoodViewModel
            {
                Date = date,
                MoodList = await GetMoodSelectList()
            };

            return dailyMoodVM;
        }

        private async Task<SelectList> GetMoodSelectList()
        {
            Dictionary<int, string> moods = await _moodService.GetMoodNameDict();
            return new SelectList(moods, "Key", "Value");
        }

        private async Task<SelectList> GetMoodSelectList(int id)
        {
            Dictionary<int, string> moods = await _moodService.GetMoodNameDict();

            if (!moods.ContainsKey(id))
            {
                return new SelectList(moods, "Key", "Value");
            }
            else
            {
                return new SelectList(moods, "Key", "Value", id);
            }
        }
    }
}
