using Application.DTOs.Pricing;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin.PricingTemplates
{
    public class PricingTemplatesIndexVm
    {
        public int? CinemaId { get; set; }
        public int? HallId { get; set; }

        public List<SelectListItem> Cinemas { get; set; } = new();
        public List<SelectListItem> Halls { get; set; } = new();

        public List<PricingTemplateListItemDto> Templates { get; set; } = new();
    }
}