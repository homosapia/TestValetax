using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestValetax.DB.Entities;
using TestValetax.DB.Repositories.Interface;
using TestValetax.Exceptions;
using TestValetax.Model;
using TestValetax.Services.Interface;

namespace TestValetax.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserJournalController : ControllerBase
    {
        private readonly IUserTokenRepository _tokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IJournalRepository _journalRepository;
        private readonly ILogger<UserJournalController> _logger;
        private readonly ITreeService _treeService;

        public UserJournalController(IJournalRepository journalRepository, 
            IUserTokenRepository tokenRepository, 
            ITokenService tokenService,
            ITreeService treeService,
            ILogger<UserJournalController> logger)
        {
            _journalRepository = journalRepository;
            _tokenRepository = tokenRepository;
            _tokenService = tokenService;
            _treeService = treeService;
            _logger = logger;
        }

        [HttpPost("api.user.journal.getRange")]
        public async Task<ActionResult<MRange_MJournalInfo>> GetRange(int skip, int take, [FromBody] VJournalFilter? filter)
        {
            var (items, totalCount) = await _journalRepository.GetFilteredAsync(skip, take, filter?.From, filter?.To, filter?.Search);

            return Ok(new MRange_MJournalInfo
            {
                Skip = skip,
                Count = totalCount,
                Items = items.Select(x => new MJournalInfo()
                {
                    Id = x.EventId,
                    EventId = x.EventId,
                    CreatedAt = x.Timestamp
                }).ToList()
            });
        }

        [HttpPost("api.user.journal.getSingle")]
        public async Task<ActionResult<MJournal>> GetSingle(int id)
        {
            var journal = await _journalRepository.GetByIdAsync(id);

            if (journal == null)
                throw new SecureException($"Journal entry with id {id} not found");

            return Ok(new MJournal
            {
                Id = journal.EventId,
                EventId = journal.EventId,
                Text = journal.StackTrace,
                CreatedAt = journal.Timestamp
            });
        }

        [HttpPost("api.user.partner.rememberMe")]
        public async Task<ActionResult<TokenInfo>> RememberMe([FromQuery] string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new SecureException("Code cannot be empty");
                }

                var existingToken = await _tokenRepository.GetByCodeAsync(code);

                if (existingToken != null && existingToken.ExpiresAt > DateTime.UtcNow)
                {
                    return Ok(new TokenInfo { Token = existingToken.Token });
                }

                await _tokenRepository.DeactivateOldTokensAsync(code);

                var newToken = _tokenService.GenerateToken(code);

                var userToken = new UserToken
                {
                    Code = code,
                    Token = newToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                };

                await _tokenRepository.AddAsync(userToken);
                await _tokenRepository.SaveChangesAsync();

                _logger.LogInformation("New token generated for code: {Code}", code);

                return Ok(new TokenInfo { Token = newToken });
            }
            catch (SecureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RememberMe for code: {Code}", code);
                throw;
            }
        }

        [HttpPost("api.user.tree.get")]
        public async Task<ActionResult<MNode>> Get(string treeName)
        {
            try
            {
                _logger.LogInformation("Getting tree: {TreeName}", treeName);

                var tree = await _treeService.GetOrCreateTreeAsync(treeName);

                return Ok(tree);
            }
            catch (SecureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tree: {TreeName}", treeName);
                throw;
            }
        }

        [HttpPost("api.user.tree.node.create")]
        public async Task<IActionResult> Create(string treeName, string nodeName, long? parentNodeId)
        {
            await _treeService.CreateNodeAsync(treeName, parentNodeId, nodeName);
            return Ok();
        }

        [HttpPost("api.user.tree.node.delete")]
        public async Task<IActionResult> Delete(long nodeId)
        {
            await _treeService.DeleteNodeAsync(nodeId);
            return Ok();
        }

        [HttpPost("api.user.tree.node.rename")]
        public async Task<IActionResult> Rename(long nodeId, string newNodeName)
        {
            await _treeService.RenameNodeAsync(nodeId, newNodeName);
            return Ok();
        }
    }
}
