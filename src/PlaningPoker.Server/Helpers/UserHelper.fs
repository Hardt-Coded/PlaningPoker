module UserHelper

open System.Net.Http
open Microsoft.Azure.Functions.Worker.Http
open System.IdentityModel.Tokens.Jwt
open System.Net.Http.Headers


let httpClient = new HttpClient()

let extractToken (req:HttpRequestData) =
    let values = req.Headers.GetValues("Authorization")
    values |> Seq.tryHead |> Option.map (fun t -> t.Replace("Bearer ",""))


let decodeToken (token:string) =
    let handler = new JwtSecurityTokenHandler();
    let jwtSecurityToken = handler.ReadJwtToken(token);
    let claims = jwtSecurityToken.Claims |> Seq.map (fun i-> (i.Type,i.Value)) |> Seq.toList
    $"%A{claims}"


let typGetEmail (token:string) =
    let handler = new JwtSecurityTokenHandler();
    let jwtSecurityToken = handler.ReadJwtToken(token);
    jwtSecurityToken.Claims |> Seq.tryFind (fun i -> i.Type = "emails") |> Option.map (fun i -> i.Value)


let getCurrentUserInfo token =
    task {
        use req = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")
        req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", token)
        let! result = httpClient.SendAsync(req)
        if result.IsSuccessStatusCode then
            return! result.Content.ReadAsStringAsync()
        else
            return! result.Content.ReadAsStringAsync()
    }

