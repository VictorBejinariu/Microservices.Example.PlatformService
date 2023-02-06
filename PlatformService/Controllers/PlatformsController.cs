using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Models;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.SyncDataServices.Http;
using PlatformService.AsyncDataServices;

namespace PlatformService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(
            IPlatformRepo repository, 
            IMapper mapper,
            ICommandDataClient commandDataClient,
            IMessageBusClient messageBusClient)
        {
            this._mapper = mapper??throw new ArgumentNullException(nameof(mapper));
            this._repository = repository??throw new ArgumentNullException(nameof(repository));
            this._commandDataClient = commandDataClient??throw new ArgumentNullException(nameof(commandDataClient));
            this._messageBusClient = messageBusClient??throw new ArgumentNullException(nameof(MessageBusClient));
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms(){
            System.Console.WriteLine("--> Getting platforms");

            var platforms = _repository.GetAllPlatforms();
            
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));            
        }

        [HttpGet("{id}", Name="GetById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id){
            var platformItem = _repository.GetPlatformById(id);
            if(platformItem == null){
                return NotFound();
            }
            return Ok(_mapper.Map<PlatformReadDto>(platformItem));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatormAsync(PlatformCreateDto inputPlatform){
            Platform entity = _mapper.Map<Platform>(inputPlatform);
            _repository.CreatePlatform(entity);
            _repository.SaveChanges();
            
            var platformReadDto = _mapper.Map<PlatformReadDto>(entity);
            //Send Sync
            try{
                await _commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch(Exception e){
                System.Console.WriteLine($"--> Could not send synchronously: {e.Message}");
            }

            //Send Async
            try
            {
                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
                platformPublishedDto.Event = "Platform_Published";
                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
            }

            return CreatedAtRoute("GetById",new {Id = platformReadDto.Id}, platformReadDto);
        }
    }
}