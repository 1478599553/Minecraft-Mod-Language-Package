using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Spider.JsonLib;

namespace Spider {
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
            return JsonConvert.DeserializeObject<ModInfo[]>(await httpClient.GetStringAsync(uriBuilder.Uri));
        }

        /// <summary>
        /// ��ȡ������ģ�����Ϣ
        /// </summary>
        /// <param name="whiteList"></param>
        /// <returns></returns>
        public static async Task<ModInfo[]> GetModInfoAsync(string[] whiteList) {
            if (whiteList.ToList().Contains("null")) return Array.Empty<ModInfo>();
            var res = new List<ModInfo>();
            foreach (var id in whiteList) {
                using var httpClient = new HttpClient();
                var uri = new Uri($"https://addons-ecs.forgesvc.net/api/v2/addon/{id}");
                var result = JsonConvert.DeserializeObject<ModInfo>(await httpClient.GetStringAsync(uri));
                res.Add(result);
            }

            return res.ToArray();
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
    }
}