using System;

using Disunity.Disinfo.Options;

using EmbedDB.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace Disunity.Disinfo.Data {

    public class DisinfoDbContext : EmbedDBContext<DisinfoDbContext> { }
    

}