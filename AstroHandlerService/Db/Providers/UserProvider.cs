﻿using AstroHandlerService.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstroHandlerService.Db.Providers
{
    public class UserProvider : IUserProvider
    {
        private ApplicationContext _appContext { get; set; }

        public UserProvider(
            ApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public async Task<User?> GetUser(long userId)
        {
            return await _appContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task AddUser(User user)
        {
            if (user == null)
            {
                return;
            }

            _appContext.Users.Add(user);
            await _appContext.SaveChangesAsync();
        }

        public async Task RemoveUser(User user)
        {
            if (user == null)
            {
                return;
            }

            _appContext.Users.Remove(user);
            await _appContext.SaveChangesAsync();
        }

        public async Task EditUser(long userId, User editInfo)
        {
            if (editInfo == null)
            {
                return;
            }

            var user = await _appContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return;
            }

            user.BirthDate = editInfo.BirthDate;
            user.GmtOffset = editInfo.GmtOffset;
            user.Language = editInfo.Language;

            _appContext.Users.Update(user);
            await _appContext.SaveChangesAsync();
        }
    }
}
