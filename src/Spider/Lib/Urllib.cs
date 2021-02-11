using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Spider.Lib.JsonLib;

namespace Spider.Lib {
    public static class UrlLib {
        /// <summary>
        /// ʹ�ö������Ϸ�汾��ģ������������ȡģ����Ϣ
        /// </summary>
        /// <param name="modCount"></param>
        /// <param name="gameVersion"></param>
        /// <returns></returns>
        public static async Task<ModInfo[]> GetModInfoAsync(int modCount, string gameVersion)
        {
            using var httpClient = new HttpClient();
            var uriBuilder = new UriBuilder("https://addons-ecs.forgesvc.net/api/v2/addon/search")
            {
                Query =
                    $"categoryId=0&gameId=432&index=0&pageSize={modCount}&gameVersion={gameVersion}&sectionId=6&sort=1"
            };
            return await httpClient.GetFromJsonAsync<ModInfo[]>(uriBuilder.Uri);
        }

        /// <summary>
        /// ��ȡ����ģ�����Ϣ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<ModInfo> GetModInfoAsync(long id) {
            var httpClient = new HttpClient();
            var uri = new Uri($"https://addons-ecs.forgesvc.net/api/v2/addon/{id}");
            var result = await httpClient.GetFromJsonAsync<ModInfo>(uri);

            return result;
        }

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Uri GetDownloadUrl(string fileId, string fileName)
        {
            return new Uri($"https://edge.forgecdn.net/files/{fileId[..4]}/{fileId[4..]}/{fileName}");
        }

        /// <summary>
        /// ��ȡ��Ŀ����
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetProjectName(Uri uri) {
            var url = uri.ToString();
            var start = url.LastIndexOf('/') + 1;
            return url[start..];
        }

        /// <summary>
        /// ������ȡǰ9999��mod��idӳ���
        /// </summary>
        /// <param name="version"></param>
        public static async void GetAllModIntroAsync(string version) {
            using var httpClient = new HttpClient();
            var uriBuilder = new UriBuilder("https://addons-ecs.forgesvc.net/api/v2/addon/search") {
                Query =
                    $"categoryId=0&gameId=432&index=0&pageSize=9999&gameVersion={version}&sectionId=6&sort=1"
            };
            var tmp = ((await httpClient.GetFromJsonAsync<ModInfo[]>(uriBuilder.Uri)) ?? Array.Empty<ModInfo>()).ToList();
            var intro = tmp.Select(_ => {
                var c = new ModIntro() {
                    Id = _.Id,
                    Name = GetProjectName(_.WebsiteUrl)
                };
                return c;
            });
            var str = JsonSerializer.SerializeToUtf8Bytes(intro, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllBytesAsync(@$"{Directory.GetCurrentDirectory()}\config\spider\{version}\intro.json",
                str);
        }

        /// <summary>
        /// ����mod
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static async Task<(Mod, bool)> DownloadAsync(ModInfo mod, string version) {
            var httpCli = new HttpClient();
            var path = $"{Path.GetTempFileName()}".Replace(".tmp", ".jar");

            var file = mod.GameVersionLatestFiles.First(_ => _.GameVersion == version);
            var downloadUrl = UrlLib.GetDownloadUrl(file.ProjectFileId.ToString(), file.ProjectFileName);
            try {
                var bytes = await httpCli.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(path, bytes);
            } catch (Exception e) {
                Serilog.Log.Logger.Error(e.ToString());
                return (null, false);
            }

            var res = new Mod() {
                Version = version,
                DownloadUrl = downloadUrl,
                Name = mod.Name,
                ProjectId = mod.Id,
                ProjectName = UrlLib.GetProjectName(mod.WebsiteUrl),
                ProjectUrl = mod.WebsiteUrl,
                TempPath = path,
            };
            return (res, true);
        }
    }
}