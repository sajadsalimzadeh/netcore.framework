using Devor.Framework.Attributes;
using Devor.Framework.Database.Abstractions;
using Devor.Framework.Entities;
using Devor.Framework.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Devor.Framework.Services
{
    [ServiceRegisterar]
    internal class LookupService : ILookupService
    {
        private readonly IEnumerable<IDbContext> dbContexts;

        public LookupService(IEnumerable<IDbContext> dbContexts)
        {
            this.dbContexts = dbContexts;
        }

        public void Initiate()
        {
            foreach (var dbContext in dbContexts)
            {
                using var transaction = dbContext.BeginTransaction();
                foreach (var type in dbContext.GetLookupEntities())
                {
                    foreach (var value in Enum.GetValues(type))
                    {
                        var name = Enum.GetName(type, value);
                        var enumValueMemberInfo = type.GetMember(name).FirstOrDefault(m => m.DeclaringType == type);
                        var descriptionAttr = enumValueMemberInfo.GetCustomAttribute<DescriptionAttribute>();
                        var rec = dbContext.DbSet<Lookup>().FirstOrDefault(x => x.Entity == type.Name && x.Key == name);
                        if (rec is null)
                        {
                            
                            dbContext.InsertEntity(new Lookup()
                            {
                                Entity = type.Name,
                                Key = name,
                                Value = Convert.ToInt32(value),
                                DisplayName = descriptionAttr?.Description
                            });
                        }
                        else if(rec.DisplayName != descriptionAttr?.Description)
                        {
                            rec.DisplayName = descriptionAttr?.Description;
                            dbContext.UpdateEntity(rec);
                        }
                    }
                }
                transaction.Commit();
            }
        }
    }
}
