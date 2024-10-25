namespace Service.KoiFishService;

using System;
using KoiBet.DTO.User;
using Microsoft.AspNetCore.Mvc;
using KoiBet.Data;
using Microsoft.AspNetCore.Authorization;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Service.JwtService;
using Microsoft.AspNetCore.Http;
using DTO.KoiFish;

public interface IKoiFishService
{
    public Task<IActionResult> HandleGetAllKoiFishes();
    public Task<IActionResult> HandleGetKoiFishById(SearchKoiDTO searchKoiDTO);
    public Task<IActionResult> HandleGetKoiFishByUserId(SearchKoiUserIdDTO searchKoiUserId);
    public Task<IActionResult> HandleCreateNewKoiFish(CreateKoiFishDTO createKoiFishDto);
    public Task<IActionResult> HandleUpdateKoiFish(UpdateKoiFishDTO updateKoiFishDto);
    public Task<IActionResult> HandleDeleteKoiFish(string koiId);
}

public class KoiFishService : ControllerBase, IKoiFishService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;

    public KoiFishService(ApplicationDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<IActionResult> HandleGetAllKoiFishes()
    {
        try
        {
            var koiFish = _context.FishKoi
                .Include(e => e.User);// Bao gồm thông tin người sở hữu

            var koiFishes = koiFish
                .Select(koiFish => new KoiFishDTO
                {
                    koi_id = koiFish.koi_id,
                    koi_name = koiFish.koi_name,
                    koi_variety = koiFish.koi_variety,
                    koi_size = koiFish.koi_size,
                    koi_age = koiFish.koi_age,
                    users_id = new UserDTO
                    {
                        user_id = koiFish.User.user_id,
                        Username = koiFish.User.Username,
                        full_name = koiFish.User.full_name,
                        email = koiFish.User.Email,
                        phone = koiFish.User.Phone,
                        role_id = koiFish.User.role_id,
                        balance = koiFish.User.Balance,
                    },
                })
                .OrderByDescending(koiFish => koiFish.koi_id)
                .ToList();

            //Nếu list null trả về badrequest
            if (koiFishes.Count() < 1)
            {
                return BadRequest("No fish!");
            }

            return Ok(koiFishes);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> HandleGetKoiFishById(SearchKoiDTO searchKoiDTO)
    {
        try
        {
            var koiFish = _context.FishKoi
                .FirstOrDefault(r => r.koi_id == searchKoiDTO.koi_id);// Bao gồm thông tin người sở hữu

            //Nếu list null trả về badrequest
            if (koiFish == null)
            {
                return BadRequest("No fish!");
            }

            var koiFishDto = new KoiFishDTO
            {
                koi_id = koiFish?.koi_id,
                koi_name = koiFish.koi_name,
                koi_age = koiFish.koi_age,
                koi_size = koiFish.koi_size,
                koi_variety = koiFish.koi_variety,
                userId = koiFish.users_id,
                //users_id = new UserDTO
                //{
                //    user_id = koiFish.User.user_id,
                //    Username = koiFish.User.Username,
                //    full_name = koiFish.User.full_name,
                //    email = koiFish.User.Email,
                //    phone = koiFish.User.Phone,
                //    role_id = koiFish.User.role_id,
                //    balance = koiFish.User.Balance,
                //},
            };

            return Ok(koiFishDto);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> HandleGetKoiFishByUserId(SearchKoiUserIdDTO searchKoiUserId)
    {
        try
        {
            var koiFish = _context.FishKoi
                .Where(r => r.users_id == searchKoiUserId.user_id)
                .Include(e => e.User);// Bao gồm thông tin người sở hữu

            //Nếu list null trả về badrequest
            if (koiFish == null)
            {
                return BadRequest("No fish!");
            }

            return new OkObjectResult(koiFish);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> HandleCreateNewKoiFish(CreateKoiFishDTO createKoiFishDto)
    {
        try
        {
            var koiFish = new FishKoi
            {
                koi_id = _jwtService.CreateNewGuid().ToString(),
                koi_name = createKoiFishDto.koi_name,
                koi_variety = createKoiFishDto.koi_variety,
                koi_size = createKoiFishDto.koi_size,
                koi_age = createKoiFishDto.koi_age,
                users_id = createKoiFishDto.userId,
            };

            var add = _context.FishKoi
                .Add(koiFish);

            var result = _context
                .SaveChanges();

            if(result != 1)
            {
                return BadRequest("Unsuccessful!");
            }

            return Ok(koiFish);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> HandleUpdateKoiFish(UpdateKoiFishDTO updateKoiFishDto)
    {
        try
        {
            var koiFish = _context.FishKoi
                .FirstOrDefault(r => r.koi_id == updateKoiFishDto.koi_id);

            if (koiFish == null)
            {
                return BadRequest("Koi fish not found!");
            }

            koiFish.koi_name = updateKoiFishDto.koi_name;
            koiFish.koi_variety = updateKoiFishDto.koi_variety;
            koiFish.koi_size = updateKoiFishDto.koi_size;
            koiFish.koi_age = updateKoiFishDto.koi_age;

            _context.FishKoi.Update(koiFish);

            var result = _context
                .SaveChanges();

            if (result != 1)
            {
                return BadRequest("Unsuccessful!");
            }

            return Ok(koiFish);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> HandleDeleteKoiFish(string koiId)
    {
        try
        {
            var koiFish = _context.FishKoi
                .FirstOrDefault(r => r.koi_id == koiId);

            if (koiFish == null)
            {
                return BadRequest("Koi fish not found!");
            }

            _context.FishKoi.Remove(koiFish);

            var result = _context
                .SaveChanges();

            if (result != 1)
            {
                return BadRequest("Unsuccessful!");
            }

            return Ok("Successful!");
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}
