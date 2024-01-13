using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MinimalAPI;
using MinimalAPI.Models;
using MinimalAPI.Models.DTO;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
{
    _logger.Log(LogLevel.Information, "Getting all Coupons");

    APIResponse response = new()
    {
        Result = CouponStore.Coupons,
        IsSuccess = true,
        StatusCode = HttpStatusCode.OK,
    };
    return Results.Ok(response);

})
    .WithName("GetCoupons").Produces<IEnumerable<APIResponse>>(200);

app.MapGet("/api/coupon/{id:int}", (ILogger<Program> _logger, int id) =>
{
    APIResponse response = new()
    {
        Result = CouponStore.Coupons.FirstOrDefault(u => u.Id == id),
        IsSuccess = true,
        StatusCode = HttpStatusCode.OK,
    };

    return Results.Ok(response);

})
    .WithName("GetCoupon")
    .Produces<APIResponse>(200);

app.MapPost("/api/coupon/", async (IMapper _mapper,
                             IValidator<CouponCreateDTO> _validation,
                             [FromBody] CouponCreateDTO coupon_C_DTO) =>
{
    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest,
    };

    var validationResult = await _validation.ValidateAsync(coupon_C_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ErrorMessage);
        return Results.BadRequest(response.ErrorMessages);
    }
    //apply the same here
    if (CouponStore.Coupons.FirstOrDefault(u => u.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already Exist");
        return Results.BadRequest(response.ErrorMessages);
    }
    Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

    coupon.Id = CouponStore.Coupons.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
    CouponStore.Coupons.Add(coupon);

    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    response.StatusCode = HttpStatusCode.Created;
    response.Result = couponDTO;
    response.IsSuccess = true;
    return Results.Ok(response);

}).WithName("CreateCoupon")
    .Accepts<CouponCreateDTO>("application/json")
    .Produces<APIResponse>(201)
    .Produces(400);


app.MapPut("/api/coupon/", async (IMapper _mapper,
                             IValidator<CouponUpdateDTO> _validation,
                             [FromBody] CouponUpdateDTO coupon_U_DTO) =>
{
    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest,
    };

    var validationResult = await _validation.ValidateAsync(coupon_U_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ErrorMessage);
        return Results.BadRequest(response.ErrorMessages);
    }


    Coupon couponFromStore = CouponStore.Coupons.FirstOrDefault(u => u.Id == coupon_U_DTO.Id);
    couponFromStore.IsActive = coupon_U_DTO.IsActive;
    couponFromStore.Name = coupon_U_DTO.Name;
    couponFromStore.Percent = coupon_U_DTO.Percent;
    couponFromStore.LastUpdated = DateTime.Now;

    response.StatusCode = HttpStatusCode.OK;
    response.Result = _mapper.Map<Coupon>(coupon_U_DTO);
    response.IsSuccess = true;
    return Results.Ok(response);

}).WithName("UpdateCoupon")
    .Accepts<CouponUpdateDTO>("application/json")
    .Produces<APIResponse>(200)
    .Produces(400);

app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest,
    };

    Coupon couponFromStore = CouponStore.Coupons.FirstOrDefault(u => u.Id == id);
    if (couponFromStore != null)
    {
        CouponStore.Coupons.Remove(couponFromStore);
        response.StatusCode = HttpStatusCode.NoContent;
        response.IsSuccess = true;

        return Results.Ok(response);
    }
    else
    {
        response.ErrorMessages.Add("Coupon does not exist!");
        return Results.BadRequest(response);
    }
});

app.UseHttpsRedirection();

app.Run();
