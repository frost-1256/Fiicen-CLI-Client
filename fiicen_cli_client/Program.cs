using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // ユーザー名を入力
        Console.Write("ユーザー名を入力してください: ");
        string username = Console.ReadLine();

        // パスワードを入力
        Console.Write("パスワードを入力してください: ");
        string password = Console.ReadLine();

        // HttpClientのインスタンスを作成
        HttpClient client;

        // HttpClientHandlerを作成
        HttpClientHandler handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            UseDefaultCredentials = false // ここでfalseに設定することが重要です
        };

        // HttpClientにハンドラを設定
        client = new HttpClient(handler);

        // ログインページのURL
        string loginUrl = "https://fiicen.jp/login/";

        // GETリクエストを送信してログインページのHTMLを取得
        var getPageResponse = await client.GetAsync(loginUrl);
        var getPageContent = await getPageResponse.Content.ReadAsStringAsync();

        // CSRFトークンをHTMLから抽出
        var csrfTokenMatch = Regex.Match(getPageContent, "<input type=\"hidden\" name=\"csrfmiddlewaretoken\" value=\"([^\"]+)\"");
        if (!csrfTokenMatch.Success)
        {
            Console.WriteLine("CSRFトークンが見つかりませんでした。");
            return;
        }
        string csrfToken = csrfTokenMatch.Groups[1].Value;

        // ログイン情報を設定
        var loginInfo = new Dictionary<string, string>
        {
            { "csrfmiddlewaretoken", csrfToken },
            { "account_name", username },
            { "password", password }
        };

        // ログイン情報をURLエンコードされた形式に変換
        var content = new FormUrlEncodedContent(loginInfo);

        // POSTリクエストを送信してログインを試行
        var response = await client.PostAsync(loginUrl, content);

        // レスポンスを確認
        if (response.StatusCode == HttpStatusCode.OK)
        {
            // ログイン成功時の処理
            Console.WriteLine("ログイン成功！");
            // クッキーを取得
            var cookies = handler.CookieContainer.GetCookies(new Uri(loginUrl));
            foreach (Cookie cookie in cookies)
            {
                Console.WriteLine($"{cookie.Name}: {cookie.Value}");
            }
            for(; ; )
            {
                // サークルを作成するためのデータを設定
                string createCircleUrl = "https://fiicen.jp/circle/create/";
                Console.WriteLine("こ↑こ↓にサークルの内容を書き込む");
                string circleContents = Console.ReadLine();

                // クッキーとCSRFトークンをヘッダーに設定
                client.DefaultRequestHeaders.Add("X-Csrftoken", csrfToken);

                // multipart/form-data形式のデータを作成
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(circleContents), "contents");

                // サークルを作成
                var createCircleResponse = await client.PostAsync(createCircleUrl, formData);

                // レスポンスを確認
                if (createCircleResponse.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("サークルの作成に成功しました！");
                }
                else
                {
                    Console.WriteLine("サークルの作成に失敗しました。");
                }
            }
        }
        else
        {
            // ログイン失敗時の処理
            Console.WriteLine("ログイン失敗！");
        }
    }
}
