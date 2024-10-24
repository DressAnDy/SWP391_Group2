﻿using KoiBet.Data;
using KoiBet.DTO.CompetitionRound;
using KoiBet.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoiBet.Service
{
    public interface ICompetitionRoundService
    {
        Task<IActionResult> HandleGetAllRounds();
        Task<IActionResult> HandleCreateNewRound(CreateCompetitionRoundDTO createRoundDto);
        Task<IActionResult> HandleUpdateRound(string roundId, UpdateCompetitionRoundDTO updateRoundDto);
        Task<IActionResult> HandleDeleteRound(string roundId);
        Task<IActionResult> HandleGetRound(string roundId);
    }

    public class CompetitionRoundService : ControllerBase, ICompetitionRoundService
    {
        private readonly ApplicationDbContext _context;

        public CompetitionRoundService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all Rounds
        public async Task<IActionResult> HandleGetAllRounds()
        {
            try
            {
                var rounds = await _context.CompetitionRounds
                    .Select(round => new CompetitionRoundDTO
                    {
                        RoundId = round.RoundId,
                        Match = round.Match,
                        CompetitionId = round.competition_id
                    })
                    .ToListAsync();

                if (!rounds.Any())
                {
                    return NotFound("No rounds found!");
                }

                return Ok(rounds);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving rounds: {ex.Message}");
            }
        }

        // Create a new Round
        public async Task<IActionResult> HandleCreateNewRound(CreateCompetitionRoundDTO createRoundDto)
        {
            try
            {
                var lastRound = await _context.CompetitionRounds
                    .OrderByDescending(r => r.RoundId)
                    .FirstOrDefaultAsync();

                int newRoundNumber = 1; // Default ID starts at 1 if no records exist

                if (lastRound != null)
                {
                    var lastId = lastRound.RoundId;
                    if (lastId.StartsWith("Round_"))
                    {
                        int.TryParse(lastId.Substring(6), out newRoundNumber); // Increment the ID number after 'Round_'
                        newRoundNumber++;
                    }
                }

                var newRound = new CompetitionRound
                {
                    RoundId = $"Round_{newRoundNumber}",
                    Match = createRoundDto.Match,
                    competition_id = createRoundDto.CompetitionId
                };

                _context.CompetitionRounds.Add(newRound);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to create new round!");
                }

                return Ok(newRound);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating round: {ex.Message}");
            }
        }

        // Update an existing Round
        public async Task<IActionResult> HandleUpdateRound(string roundId, UpdateCompetitionRoundDTO updateRoundDto)
        {
            try
            {
                var round = await _context.CompetitionRounds
                    .FirstOrDefaultAsync(r => r.RoundId == roundId);

                if (round == null)
                {
                    return NotFound("Round not found!");
                }

                round.Match = updateRoundDto.Match;
                round.competition_id = updateRoundDto.CompetitionId;

                _context.CompetitionRounds.Update(round);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to update round!");
                }

                return Ok(round);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating round: {ex.Message}");
            }
        }

        // Delete a Round
        public async Task<IActionResult> HandleDeleteRound(string roundId)
        {
            try
            {
                var round = await _context.CompetitionRounds
                    .FirstOrDefaultAsync(r => r.RoundId  == roundId);

                if (round == null)
                {
                    return NotFound("Round not found!");
                }

                _context.CompetitionRounds.Remove(round);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to delete round!");
                }

                return Ok("Round deleted successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting round: {ex.Message}");
            }
        }

        // Get a specific Round by ID
        public async Task<IActionResult> HandleGetRound(string roundId)
        {
            try
            {
                var round = await _context.CompetitionRounds
                    .Select(r => new CompetitionRoundDTO
                    {
                        RoundId = r.RoundId,
                        Match = r.Match,
                        CompetitionId = r.competition_id
                    })
                    .FirstOrDefaultAsync(r => r.RoundId == roundId);

                if (round == null)
                {
                    return NotFound("Round not found!");
                }

                return Ok(round);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving round: {ex.Message}");
            }
        }
    }
}
