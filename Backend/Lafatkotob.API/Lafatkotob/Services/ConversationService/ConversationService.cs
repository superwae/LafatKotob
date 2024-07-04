using Lafatkotob.Entities;
using Lafatkotob.Hubs;
using Lafatkotob.Services.ConversationService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.ConversationService
{
    public class ConversationService : IConversationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;


        public ConversationService(ApplicationDbContext context, UserManager<AppUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<ServiceResponse<ConversationModel>> Post(ConversationModel model)
        {
            var response = new ServiceResponse<ConversationModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var conversation = new Conversation
                        {
                            LastMessageDate = model.LastMessageDate,
                            LastMessage = model.LastMessage,


                        };

                        _context.Conversations.Add(conversation);
                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        model.Id = conversation.Id;
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create badge.";
                    }
                }
            });
            return response;
        }

        public async Task<ConversationModel> GetById(int id)
        {
            var conversation = await _context.Conversations.FindAsync(id);
            if (conversation == null) return null;

            return new ConversationModel
            {
                Id = conversation.Id,
                LastMessageDate = conversation.LastMessageDate,
                LastMessage = conversation.LastMessage
            };
        }

        public async Task<List<ConversationModel>> GetAll()
        {
            return await _context.Conversations
                .AsNoTracking()
                .Select(c => new ConversationModel
                {
                    Id = c.Id,
                    LastMessageDate = c.LastMessageDate,
                    LastMessage = c.LastMessage,

                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<ConversationModel>> Update(ConversationModel model)
        {
            var response = new ServiceResponse<ConversationModel>();

            if (model == null) {
                response.Success = false;
                response.Message = "Model is null";
                return response;
            }
            var Conversation = await _context.Conversations.FindAsync(model.Id);
            if (Conversation == null)
            {
                response.Success = false;
                response.Message = "conversation  not found.";
                return response;
            }
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {

                        Conversation.LastMessageDate = DateTime.Now;
                        Conversation.LastMessage = model.LastMessage;

                        _context.Conversations.Update(Conversation);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update badge: {ex.Message}";
                    }
                }
            });
            return response;
        }

        public async Task<ServiceResponse<ConversationModel>> Delete(int id)
        {
            var response = new ServiceResponse<ConversationModel>();
            var Conversation = await _context.Conversations.FindAsync(id);
            if (Conversation == null)
            {
                response.Success = false;
                response.Message = "Badge not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {


                        _context.Conversations.Remove(Conversation);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new ConversationModel
                        {
                            Id = Conversation.Id,
                            LastMessageDate = Conversation.LastMessageDate,
                            LastMessage = Conversation.LastMessage
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete badge: {ex.Message}";
                    }
                }
            });
            return response;
        }


        public async Task<ServiceResponse<List<ConversationsBoxModel>>> GetConversationsForUser(string userId)
        {
            var response = new ServiceResponse<List<ConversationsBoxModel>>();
            var userConversations = await _context.ConversationsUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Conversation)
                .ThenInclude(c => c.Messages)
                .Include(cu => cu.Conversation)
                .ThenInclude(c => c.ConversationsUsers)
                .ThenInclude(cu => cu.AppUser)
                .Select(cu => cu.Conversation)
                .Distinct()
                .OrderByDescending(c => c.LastMessageDate)
                .ToListAsync();


            var viewModels = new List<ConversationsBoxModel>();

            foreach (var conversation in userConversations)
            {
                var otherUser = conversation.ConversationsUsers
                    .FirstOrDefault(cu => cu.UserId != userId)?.AppUser;

                if (otherUser != null)
                {
                    var viewModel = new ConversationsBoxModel
                    {
                        ConversationId = conversation.Id,
                        LastMessage = conversation.LastMessage,
                        LastMessageDate = conversation.LastMessageDate,
                        UserId = otherUser.Id,
                        UserName = otherUser.UserName,
                        LastMessageSender = conversation.LastMessageSender,
                        UserProfilePicture = ConvertToFullUrl(otherUser.ProfilePicture),
                        HasUnreadMessages = conversation.HasUnreadMessages,

                    };

                    viewModels.Add(viewModel);
                }
            }
            if (viewModels.Count == 0)
            {
                response.Success = false;
                response.Message = "No conversations found.";
                return response;
            }
            response.Success = true;
            response.Data = viewModels;


            return response;
        }


        public async Task<ServiceResponse<List<Message>>> GetAllMessagesByConversationIdAsync(int conversationId, int pageNumber = 1, int pageSize = 20)
        {
            var response = new ServiceResponse<List<Message>>();

            try
            {
                var messages = await _context.Messages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderByDescending(m => m.DateSent) // Order by descending to get the most recent messages first
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                response.Data = messages;
                response.Success = true;
                response.Message = messages.Count > 0 ? "Messages retrieved successfully." : "No messages found.";
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred while retrieving messages: {ex.Message}";
                response.Success = false;
            }

            return response;
        }

        private string ConvertToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            // Use your API's base URL here
            var baseUrl = "https://localhost:7139";
            return $"{baseUrl}{relativePath}";
        }

        public async Task<ServiceResponse<bool>> PostNewConversation(ConversationWithIdsModel model)
        {
            var response = new ServiceResponse<bool>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    //check if conversation already exists by checking if the conversation exists
                    //with the same users where we should find 2 conversationsUser
                    //with the same ids as in the model and both these conversationsUser
                    //have the same conversation id 
                    var conversationExists = await _context.ConversationsUsers
                        .Where(cu => model.UserIds.Contains(cu.UserId))
                        .GroupBy(cu => cu.ConversationId)
                        .Select(g => new
                        {
                            ConversationId = g.Key,
                            Count = g.Count()
                        })
                        .FirstOrDefaultAsync(g => g.Count == model.UserIds.Count);

                    if (conversationExists != null)
                    {

                        response.Success = true;
                        response.Message = "Conversation already exists";
                        response.Data = true;
                        return response;
                    }

                    Conversation conversation;

                    try
                    {
                        conversation = new Conversation
                        {
                            LastMessageDate = DateTime.Now,
                            LastMessage = model.LastMessage,
                            HasUnreadMessages = true, 
                            LastMessageSender = model.LastMessageSender,
                        };
                        _context.Conversations.Add(conversation);
                        await _context.SaveChangesAsync();

                        foreach (var userId in model.UserIds)
                        {
                            var conversationUser = new ConversationsUser
                            {
                                ConversationId = conversation.Id,
                                UserId = userId
                            };
                            _context.ConversationsUsers.Add(conversationUser);
                        }
                        var user1 = await _userManager.FindByIdAsync(model.UserIds[0]);
                        var user2 = await _userManager.FindByIdAsync(model.UserIds[1]);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        await _hubContext.Clients.User(user1.Id.ToString()).SendAsync("ConversationCreated", new {
                            lastMessageDate = DateTime.Now,
                            lastMessage = model.LastMessage,
                            userId = user1.Id,
                            userName = user1.UserName,
                            userProfilePicture = ConvertToFullUrl(user1.ProfilePicture),
                        });
                        await _hubContext.Clients.User(user2.Id.ToString()).SendAsync("ConversationCreated", new
                        {
                            lastMessageDate = DateTime.Now,
                            lastMessage = model.LastMessage,
                            userId = user2.Id,
                            userName = user2.UserName,
                            userProfilePicture = ConvertToFullUrl(user2.ProfilePicture),
                        });

                        // Populate the response object
                        var otheruser = await _userManager.FindByIdAsync(model.UserIds[1]);
                        response.Data = true;
                        response.Success = true;
                        return response;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Data = false;
                        response.Message = $"Failed to create conversation: {ex.Message}";
                        return response;
                    }
                }
            });
            return response;
        }

        public async Task<ServiceResponse<List<ConversationsBoxModel>>> getUsersForConversation(int id)
        {
            var response = new ServiceResponse<List<ConversationsBoxModel>>();
            var conversation = await _context.ConversationsUsers
                .Where(cu => cu.ConversationId == id)
                .Include(cu => cu.AppUser)
                .Select(cu => new ConversationsBoxModel
                {
                    ConversationId = cu.ConversationId,
                    UserId = cu.UserId,
                    UserName = cu.AppUser.UserName,
                    UserProfilePicture = ConvertToFullUrl(cu.AppUser.ProfilePicture),
                    HasUnreadMessages = cu.Conversation.HasUnreadMessages,
                })
                .ToListAsync();
            if (conversation.Count == 0)
            {
                response.Success = false;
                response.Message = "No users found.";
                return response;
            }
            response.Data = conversation;
            response.Success = true;
            return response;
        }



        public async Task<ServiceResponse<int>> ConversationCountWithUnreadMessages(string userId)
        {
            var response = new ServiceResponse<int>();
            var conversation = await _context.ConversationsUsers
                .Where(cu => cu.UserId == userId &&
                cu.Conversation.HasUnreadMessages == true&&
                cu.Conversation.LastMessageSender!=userId)
                .CountAsync();
            response.Data = conversation;
            response.Success = true;
            return response;
        }
        public async Task<ServiceResponse<ConversationModel>> MarkConversationAsRead(int conversationId, string userId)
        {
            var response = new ServiceResponse<ConversationModel>();
            var conversation = await _context.Conversations.Where(c=>c.LastMessageSender!=userId)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conversation == null)
            {
                response.Success = false;
                response.Message = "Conversation not found.";
                return response;
            }
            conversation.HasUnreadMessages = false;
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
            response.Data = new ConversationModel
            {
                Id = conversation.Id,
                LastMessageDate = conversation.LastMessageDate,
                LastMessage = conversation.LastMessage,
                LastMessageSender = conversation.LastMessageSender
            };
            response.Success = true;
            return response;

        }


    }
}
