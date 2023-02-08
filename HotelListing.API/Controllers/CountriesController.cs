using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Core.Models.Hotel;
using AutoMapper;
using HotelListing.API.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Core.Exceptions;
using Google.Apis.Admin.Directory.directory_v1.Data;
using HotelListing.API.Core.Models;

namespace HotelListing.API.Controllers
{
    [Route("v{version:apiVersion}/countries")]
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    
    public class CountriesController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesController> _logger;
        public CountriesController(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesController> logger)
        {

            this._logger = logger;
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
        }

        // GET: api/Countries/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync<GetCountryDto>();
            return Ok(countries);
        }

        //My URL should be looking like this
        // GET: api/Countries/?StartIndex=0&pagesize=25&PageNumber=1
        [HttpGet]
        public async Task<ActionResult<PagedResult <GetCountryDto>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
        {
            var pagedCountriesResult = await _countriesRepository.GetAllAsync<GetCountryDto>(queryParameters);//This returns the paged results of type getcountriesDto
            return Ok(pagedCountriesResult);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            //This is basically more like an inner join
            var country = await _countriesRepository.GetDetails(id);
            return Ok(country);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid Id specified");
            }
            try
            {
                await _countriesRepository.UpdateAsync(id, updateCountryDto);
            }  
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
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

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CountryDto>> PostCountry(CreateCountryDto createCountryDto)
        {
            var country = await _countriesRepository.AddAsync<CreateCountryDto, GetCountryDto>(createCountryDto);
            return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            await _countriesRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);
        }
    }
}
