using Application.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

public class HallsController : Controller
{
    private readonly IHallService _hallService;

    public HallsController(IHallService hallService)
    {
        _hallService = hallService;
    }
}
