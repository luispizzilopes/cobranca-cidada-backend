using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobranca.Cidada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfanityWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfanityWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfanityWords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfanityWords_Word",
                table: "ProfanityWords",
                column: "Word",
                unique: true);

            string[] portuguese =
            {
            "arrombada", "arrombadas", "arrombado", "arrombados", "arrombar", "b0sta", "babaca", "babacao",
            "babacas", "baitola", "bichona", "boceta", "bocetas", "boiola", "boquete", "bosta", "bostao",
            "bostas", "bostinha", "buceta", "bucetao", "bucetas", "c4ralho", "cacetada", "cacetao", "cacete",
            "cagada", "cagadas", "cagado", "cagalhao", "cagando", "cagao", "cagar", "cagou", "carai", "caraio",
            "caralhada", "caralho", "caralhos", "caralhudo", "caralio", "corna", "cornao", "corno", "cornos",
            "cretina", "cretino", "cretinos", "crioula", "crioulo", "cu", "cus", "cuzao", "cuzinho", "cuzona",
            "cuzudo", "desgracada", "desgracado", "desgracados", "escrota", "escrotao", "escrotas", "escroto",
            "escrotos", "f0da", "f0der", "fdp", "fdps", "filha da puta", "filhadaputa", "filho da puta",
            "filhodaputa", "filhos da puta", "foda", "foda se", "fodam", "fodao", "fodas", "fodase", "fodasse",
            "fodendo", "fodeo", "foder", "fodeu", "fodi", "fodida", "fodidas", "fodido", "fodidos", "fodona",
            "fudendo", "fuder", "fudeu", "fudi", "fudida", "fudido", "idiota", "idiotas", "imbecil", "imbecis",
            "karalho", "kralho", "krl", "lazarento", "m3rd4", "m3rda", "merda", "merdada", "merdao", "merdas",
            "merdinha", "merdosa", "merdoso", "mongol", "mongoloide", "otaria", "otarias", "otario", "otarios",
            "p0rra", "perereca", "piroca", "pirocao", "pirocas", "poha", "porra", "porrada", "porrao",
            "porras", "porrinha", "ppk", "pqp", "punheta", "punheteiro", "put4", "puta", "puta merda",
            "puta que o pariu", "puta que pariu", "putamerda", "putaria", "putas", "puteiro", "putinha",
            "puto", "putona", "putos", "retardada", "retardadas", "retardado", "retardados", "sapatao",
            "siririca", "tnc", "tomar no cu", "vadia", "vadias", "vai a merda", "vai pra merda",
            "vai pra puta que pariu", "vai se foder", "vai se fuder", "vai tomar no cu", "vi4do", "viada",
            "viadao", "viadinho", "viado", "viados", "vsf", "vtnc", "xereca", "xota", "xoxota"
            };

            string[] english =
            {
            "arse", "arsehole", "ass", "asses", "asshole", "assholes", "b1tch", "bastard", "bastards",
            "beaner", "biatch", "bitch", "bitches", "bitching", "bitchy", "blowjob", "blowjobs", "bollocks",
            "bugger", "bullshit", "bullshitter", "chink", "cock", "cocks", "cocksucker", "cocksucking", "crap",
            "crappy", "cum", "cumshot", "cunt", "cunts", "dammit", "damn", "damned", "dick", "dickhead",
            "dickheads", "dicks", "dildo", "dildos", "dipshit", "douche", "douchebag", "douchebags", "dumbass",
            "dumbfuck", "dyke", "f0ck", "fag", "faggot", "faggots", "fags", "fatass", "fck", "fucc", "fuck",
            "fuck off", "fuck you", "fucked", "fucker", "fuckers", "fuckface", "fuckhead", "fuckin", "fucking",
            "fucks", "fuckup", "fuckwit", "fuk", "fuq", "fvck", "go fuck yourself", "goddammit", "goddamn",
            "goddamned", "gook", "gtfo", "handjob", "horseshit", "idiot", "idiots", "imbecile", "jackass",
            "jackoff", "jerkoff", "jizz", "kike", "mofo", "moron", "morons", "motherfucker", "motherfuckers",
            "motherfucking", "nigga", "niggas", "nigger", "niggers", "paki", "phuck", "piss", "piss off",
            "pissed", "pissing", "pissoff", "prick", "pricks", "pussies", "pussy", "retard", "retarded",
            "retards", "screw you", "scumbag", "sh1t", "shit", "shite", "shitface", "shithead", "shits",
            "shitshow", "shitting", "shitty", "sht", "skank", "slut", "sluts", "slutty", "smartass",
            "son of a bitch", "sonofabitch", "spic", "stfu", "tits", "titties", "tosser", "towelhead",
            "tranny", "twat", "twats", "wank", "wanker", "wankers", "wetback", "whore", "whores", "wtf"
            };

            foreach (var (language, words) in new[] { ("pt", portuguese), ("en", english) })
            {
                foreach (var word in words)
                {
                    migrationBuilder.InsertData(
                        table: "ProfanityWords",
                        columns: new[] { "Id", "Word", "Language", "IsActive", "CreatedAtUtc" },
                        values: new object[] { Guid.NewGuid(), word, language, true, DateTime.UtcNow });
                }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfanityWords");
        }
    }
}
