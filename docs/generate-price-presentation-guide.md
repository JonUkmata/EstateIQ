# Generate Price - shpjegim per prezantim

Ky dokument shpjegon rrjedhen e funksionit **Generate Price** ne EstateIQ prej momentit kur perdoruesi shtyp butonin ne frontend, deri ne momentin kur backend-i ia dergon kerkesen ML service dhe e kthen pergjigjen ne UI.

Nuk shpjegohet logjika e brendshme e modelit ML, sepse ajo eshte pjese e nje repo-je tjeter. Ketu shpjegohet vetem integrimi: cfare mbledh frontendi, cfare validon backend-i, si e perkthen payload-in per ML, si trajtohen te dhenat opsionale, dhe si kthehet rezultati.

## Qellimi i funksionit

Qellimi nuk eshte qe agjenti te plotesoje nje forme teknike per ML. Qellimi eshte qe agjenti te plotesoje te dhena normale te nje prone, ndersa backend-i i kthen ato ne formatin qe ML service e kupton.

Pra kemi ndarje pergjegjesish:

- Frontendi mbledh te dhenat ne forme te kuptueshme per agjentin.
- Backend-i validon, normalizon dhe mapon keto te dhena.
- ML service jep vetem nje cmim te parashikuar.
- Frontendi e shfaq cmimin si sugjerim, jo si vendim final.

Cmimi i gjeneruar nuk e krijon automatikisht pronen dhe nuk e bllokon agjentin. Agjenti mund ta aplikoje me `Apply to Price`, por mund ta ndryshoje manualisht para se ta krijoje pronen.

## Arkitektura e rrjedhes

Rrjedha kryesore eshte:

```text
Create Property page
  -> Generate Price button
  -> frontend/src/pages/CreatePropertyPage.tsx
  -> frontend/src/services/api.ts
  -> POST /api/properties/generate-price
  -> backend/EstateIQ/Controllers/PropertiesController.cs
  -> backend/EstateIQ/Services/PropertyPricePredictionService.cs
  -> POST ML endpoint, p.sh. http://127.0.0.1:8000/predict
  -> backend merr predicted_price
  -> backend kthen suggestedPrice/formattedPrice/mlWarnings
  -> frontend shfaq rezultatin ne Generate Price card
```

Frontendi nuk e thirr ML service direkt. Kjo eshte me rendesi per siguri, kontroll validimi dhe per te mos e lidhur UI-ne me kontraten teknike te ML-se.

## Pse frontendi nuk e thirr ML direkt

Ka disa arsye praktike:

- Siguria: backend-i kontrollon authentication dhe authorization. Endpoint-i `POST /api/properties/generate-price` kerkon permission `CreateProperty`.
- Validimi: backend-i ka validim te plote edhe nese dikush e anashkalon frontend-in dhe dergon request direkt me Postman ose script.
- Stabiliteti i kontrates: frontendi perdor fusha agent-friendly si `livingArea`, `hasBasement`, `renovated`; ML kerkon fusha teknike si `sqft_living`, `sqft_above`, `yr_renovated`.
- Mbrojtja nga ndryshimet: nese ML contract ndryshon, mund te ndryshohet backend mapping pa ndryshuar gjithe UI-ne.
- Logging dhe error handling: backend-i logon request-in drejt ML dhe response-in, dhe e kthen gabimin ne mesazh te kuptueshem per UI.

## Hapi 1: perdoruesi ploteson formen

Forma eshte ne `frontend/src/pages/CreatePropertyPage.tsx`.

Seksionet kryesore jane:

- Listing Info
- Location
- Property Details
- Optional Accuracy Details
- Generate Price
- Final Price

Per Generate Price nuk duhen te gjitha fushat e krijimit te prones. Per shembull, `title`, `description`, `company`, `agent`, `propertyType` dhe `propertyStatus` jane te rendesishme per krijimin e prones, por nuk dergohen te ML per gjenerim cmimi.

Fushat minimale qe duhen per gjenerim cmimi jane:

- Living Area
- Bedrooms
- Bathrooms
- Floors
- Year Built
- Latitude
- Longitude
- Zipcode

Keto kontrollohen nga funksioni `getMissingPriceFields(form)`. Nese mungon ndonjera, UI shfaq `Needed: ...` dhe butoni `Generate Price` nuk eshte gati.

## Hapi 2: Location dhe map picker

Forma ka nje harte me Leaflet. Kur agjenti klikon ne harte:

- vendoset `latitude`
- vendoset `longitude`
- tentohet reverse geocoding per te mbushur `address`, `city`, `zipcode`

Reverse geocoding do te thote: nga koordinatat gjeografike tentojme te gjejme adrese/city/zipcode.

Nese reverse geocoding deshton, gjenerimi nuk ndalet automatikisht. UI vendos nje mesazh qe lokacioni mund te plotesohet manualisht. Per ML jane kritike `latitude`, `longitude` dhe `zipcode`.

## Hapi 3: perdoruesi shtyp Generate Price

Kur klikohet butoni, thirret `handleGeneratePrice()` ne `CreatePropertyPage.tsx`.

Ky funksion ben keto hapa:

1. Thirr `validatePriceFields(form)`.
2. Nese ka gabime, vendos `priceState = error` dhe shfaq mesazhin `Add the missing property details before generating a price.`
3. Nese validimi kalon, vendos `priceState = loading`.
4. Nderton payload-in me `buildPricePayload(form)`.
5. Thirr `generatePropertyPrice(payload)` nga `frontend/src/services/api.ts`.
6. Nese pergjigja eshte OK, ruan rezultatin ne `generatedPrice` dhe vendos `priceState = success`.
7. Nese ka gabim nga backend/network, vendos `priceState = error` dhe shfaq mesazhin.

`priceState` kontrollon UI-ne:

- `idle`: ende nuk eshte gjeneruar cmim.
- `loading`: kerkesa eshte duke u bere.
- `success`: cmimi u kthye me sukses.
- `error`: validimi ose request-i deshtoi.

## Hapi 4: validimi ne frontend

Frontendi validon para se te dergoje kerkesen. Ky validim eshte per UX, qe perdoruesi ta marre gabimin menjehere pa pritur backend-in.

Rregullat kryesore ne frontend jane:

- `livingArea` duhet te jete me e madhe se 0.
- `bedrooms` duhet te jete integer nga 1 deri 10.
- `bathrooms` duhet te jete numer nga 0.5 deri 8.
- `floors` duhet te jete numer nga 1 deri 3.5.
- `yearBuilt` duhet te jete nga 1800 deri ne vitin aktual + 2.
- `latitude` duhet te jete nga -90 deri 90.
- `longitude` duhet te jete nga -180 deri 180.
- `zipcode` duhet te jete integer nga 1 deri 99999.
- Nese `hasBasement = true` dhe eshte dhene `basementArea`, basement nuk guxon te jete me i madh se living area.
- Nese `renovated = true`, `yearRenovated` duhet te jete nga `yearBuilt` deri ne vitin aktual.

Ky validim nuk eshte mjaftueshem per siguri, sepse frontendi mund te anashkalohet. Prandaj backend-i e perserit validimin.

## Hapi 5: payload-i qe frontendi ia dergon backend-it

Funksioni `buildPricePayload(form)` nderton JSON-in per backend.

Shembull:

```json
{
  "livingArea": 1960,
  "livingAreaUnit": "sqft",
  "bedrooms": 4,
  "bathrooms": 3,
  "floors": 1,
  "yearBuilt": 1965,
  "latitude": 47.520801,
  "longitude": -122.393001,
  "zipcode": 98136,
  "lotArea": 5000,
  "lotAreaUnit": "sqft",
  "condition": 5,
  "grade": 7,
  "hasBasement": true,
  "basementArea": 910,
  "basementAreaUnit": "sqft",
  "waterfront": false,
  "viewQuality": 0,
  "renovated": false,
  "yearRenovated": null,
  "nearbyLivingArea": 1360,
  "nearbyLivingAreaUnit": "sqft",
  "nearbyLotArea": 5000,
  "nearbyLotAreaUnit": "sqft"
}
```

Kjo eshte ende kontrate e backend-it, jo kontrate direkte e ML-se.

## Hapi 6: api.ts dergon HTTP request

Ne `frontend/src/services/api.ts`, funksioni eshte:

```ts
export async function generatePropertyPrice(payload: GeneratePropertyPricePayload) {
  return fetchJson<GeneratePropertyPriceResponse>('/api/properties/generate-price', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}
```

`fetchJson` ben disa gjera te rendesishme:

- vendos `Accept: application/json`
- vendos `Authorization: Bearer <token>` nese perdoruesi eshte i kycur
- e dergon request-in me `fetch`
- nese status code nuk eshte OK, lexon error details dhe nderton mesazh per UI
- per `401` kthen mesazh per session expired
- per `403` kthen mesazh per permission
- per `500+` kthen mesazh server error

Ne development, Vite proxy e dergon `/api` drejt backend-it. Konfigurimi eshte ne `frontend/vite.config.ts`, ku default target eshte `http://127.0.0.1:5222`.

## Hapi 7: backend controller e pranon kerkesen

Endpoint-i eshte ne `backend/EstateIQ/Controllers/PropertiesController.cs`:

```http
POST /api/properties/generate-price
```

Metoda quhet `GeneratePrice`.

Karakteristikat:

- Ka `[HttpPost("generate-price")]`.
- Ka `[Authorize(Policy = Permissions.CreateProperty)]`.
- Pranon `GeneratePropertyPriceRequestDto`.
- Thirr `IPropertyPricePredictionService.GenerateAsync`.
- Kthen `200 OK` me `GeneratePropertyPriceResponseDto` nese gjithcka kalon.
- Kthen `400 Bad Request` nese ka validation errors.
- Kthen `503 Service Unavailable` nese ML service nuk eshte i arritshem ose deshton.

Kjo do te thote: pa login/permission te duhur, kerkesa nuk shkon fare deri te ML.

## Hapi 8: DTO qe backend pranon

DTO eshte `backend/EstateIQ/DTOs/GeneratePropertyPriceRequestDto.cs`.

Fushat kryesore:

- `LivingArea`
- `LivingAreaUnit`
- `Bedrooms`
- `Bathrooms`
- `Floors`
- `YearBuilt`
- `Latitude`
- `Longitude`
- `Zipcode`

Fushat opsionale:

- `LotArea`
- `LotAreaUnit`
- `Condition`
- `Grade`
- `HasBasement`
- `BasementArea`
- `BasementAreaUnit`
- `Waterfront`
- `ViewQuality`
- `Renovated`
- `YearRenovated`
- `NearbyLivingArea`
- `NearbyLivingAreaUnit`
- `NearbyLotArea`
- `NearbyLotAreaUnit`

Termi DTO do te thote **Data Transfer Object**. Eshte klase qe perdoret per te transportuar te dhena ne API. Nuk eshte database model dhe nuk eshte ML model; eshte kontrate e request-it.

## Hapi 9: backend service validon request-in

Logjika kryesore eshte ne `backend/EstateIQ/Services/PropertyPricePredictionService.cs`.

Metoda `GenerateAsync` ben:

1. Merr vitin aktual me `DateTime.UtcNow`.
2. Thirr `ValidateRequest(request, currentDate.Year)`.
3. Nderton ML request me `BuildMlRequest(request, currentDate)`.
4. Lexon endpoint-in nga konfigurimi `Ml:PredictUrl`, ose perdor default `http://127.0.0.1:8000/predict`.
5. E dergon ML request me `HttpClient.PostAsJsonAsync`.
6. Lexon ML response.
7. E kthen ne `GeneratePropertyPriceResponseDto`.

Validimi ne backend eshte me i rendesishem se validimi ne frontend, sepse backend-i eshte kufiri i besueshem i sistemit.

## Cfare pranohet dhe cfare nuk pranohet

Keto jane rregullat kryesore te backend-it:

| Fusha | Pranohet | Nuk pranohet | Arsyeja |
| --- | --- | --- | --- |
| `LivingArea` | > 0 | 0 ose negative | Modeli nuk mund te vleresoje prone pa siperfaqe reale |
| `LivingAreaUnit` | `m2` ose `sqft` | njesi tjeter | Backend di te konvertoje vetem keto njesi |
| `Bedrooms` | 1-10 | 0, negative, >10 | Kufizon input absurd/jashte domain-it |
| `Bathrooms` | 0.5-8 | <0.5 ose >8 | Banjo mund te jete 0.5, por jo 0 per kete flow |
| `Floors` | 1-3.5 | <=0 ose >3.5 | Perputhje me domain-in e modelit |
| `YearBuilt` | 1800 deri currentYear + 2 | shume i vjeter ose shume ne te ardhmen | Lejon pak projekte te reja, por bllokon vlera joreale |
| `Latitude` | -90 deri 90 | jashte intervalit | Kufij gjeografike reale |
| `Longitude` | -180 deri 180 | jashte intervalit | Kufij gjeografike reale |
| `Zipcode` | > 0 | 0 ose negative | Zipcode duhet te ekzistoje si numer |
| `LotArea` | > 0 nese jepet | 0 ose negative | Lot area opsionale, por nese jepet duhet te kete kuptim |
| `Condition` | 1-5 | jashte 1-5 | Shkalle cilesie e kufizuar |
| `Grade` | 1-13 | jashte 1-13 | Shkalle grade e modelit |
| `ViewQuality` | 0-4 | jashte 0-4 | 0 do te thote pa view |
| `BasementArea` | > 0 dhe <= living area | negative ose me e madhe se living area | Basement nuk mund te jete me i madh se siperfaqja totale e jetueshme |
| `YearRenovated` | >= yearBuilt dhe <= currentYear | para ndertimit ose ne te ardhmen | Renovimi nuk mund te ndodhe para ndertimit |
| Nearby area | > 0 nese jepet | 0 ose negative | E dhene krahasuese duhet te kete kuptim |

## Hapi 10: konvertimi i njesive

ML contract kerkon siperfaqet ne square feet. Prandaj backend-i i konverton te gjitha area fields ne sqft.

Formula:

```text
if unit == m2:
  sqft = round(value * 10.7639)

if unit == sqft:
  sqft = round(value)
```

Fushat qe konvertohen:

- `LivingArea` -> `sqft_living`
- `LotArea` -> `sqft_lot`
- `BasementArea` -> `sqft_basement`
- `NearbyLivingArea` -> `sqft_living15`
- `NearbyLotArea` -> `sqft_lot15`

Shembull:

```text
LivingArea = 100 m2
100 * 10.7639 = 1076.39
round = 1076 sqft
```

Kjo eshte arsyeja pse frontendi mund te lejoje `m2`, por ML merr gjithmone `sqft`.

## Hapi 11: backend nderton ML payload

Backend-i e kthen DTO-ne agent-friendly ne kontraten teknike te ML-se.

Shembull ML request:

```json
{
  "bedrooms": 4,
  "bathrooms": 3,
  "sqft_living": 1960,
  "sqft_lot": 5000,
  "floors": 1,
  "waterfront": 0,
  "view": 0,
  "condition": 5,
  "grade": 7,
  "sqft_above": 1050,
  "sqft_basement": 910,
  "yr_built": 1965,
  "yr_renovated": 0,
  "zipcode": 98136,
  "lat": 47.520801,
  "long": -122.393001,
  "sqft_living15": 1360,
  "sqft_lot15": 5000,
  "sale_year": 2014,
  "sale_month": 11
}
```

Mapping kryesor:

| Frontend/backend field | ML field | Shpjegim |
| --- | --- | --- |
| `Bedrooms` | `bedrooms` | Numri i dhomave te gjumit |
| `Bathrooms` | `bathrooms` | Numri i banjove |
| `LivingArea` | `sqft_living` | Siperfaqja e jetueshme ne sqft |
| `LotArea` | `sqft_lot` | Siperfaqja e parceles ne sqft |
| `Floors` | `floors` | Numri i kateve |
| `Waterfront` | `waterfront` | Boolean kthehet ne 1 ose 0 |
| `ViewQuality` | `view` | Cilesia e pamjes, 0-4 |
| `Condition` | `condition` | Gjendja e prones, 1-5 |
| `Grade` | `grade` | Grade teknike, 1-13 |
| `LivingArea - BasementArea` | `sqft_above` | Siperfaqja mbi nivelin e basement |
| `BasementArea` | `sqft_basement` | Basement ne sqft |
| `YearBuilt` | `yr_built` | Viti i ndertimit |
| `YearRenovated` | `yr_renovated` | 0 nese nuk eshte renovuar |
| `Zipcode` | `zipcode` | Kodi postar |
| `Latitude` | `lat` | Gjeresia gjeografike |
| `Longitude` | `long` | Gjatesia gjeografike |
| `NearbyLivingArea` | `sqft_living15` | Mesatarja/krahasimi per prona afer |
| `NearbyLotArea` | `sqft_lot15` | Parcelat afer |
| konstante backend | `sale_year` | Gjithmone 2014 |
| konstante backend | `sale_month` | Gjithmone 11 |

## Pse ekzistojne `sale_year = 2014` dhe `sale_month = 11`

Keto jane konstante ne backend:

```text
ModelSaleYear = 2014
ModelSaleMonth = 11
```

Arsyeja teknike eshte se kontrata e ML-se i kerkon keto fusha. Backend-i nuk ia kerkon agjentit sepse agjenti po krijon listing sot, jo nje shitje historike. Per te mbajtur kontraten kompatibile me modelin, backend-i i vendos vete vlerat.

Ne prezantim mund ta thuash keshtu: "Keto fusha jane pjese e dataset/contract-it te modelit. Ne nuk ia ekspozojme perdoruesit sepse nuk jane pjese e eksperiences se krijimit te listing-ut. Backend-i i vendos si konstante per te mbajtur ML request valid."

## Si mbushen fushat opsionale kur mungojne

Backend-i nuk e bllokon request-in vetem pse mungojne fushat opsionale. Ai vendos defaults te arsyeshme.

| Nese mungon | Backend vendos | Pse |
| --- | --- | --- |
| `LotArea` | `sqft_lot = sqft_living` | Me mire nje vlere fallback sesa request i paplote per ML |
| `Condition` | `condition = 3` | 3 eshte average/default |
| `Grade` | `grade = 7` | 7 eshte grade mesatare/default |
| `HasBasement` ose `BasementArea` | `sqft_basement = 0` | Supozohet pa basement |
| `Waterfront` | `waterfront = 0` | Supozohet jo waterfront |
| `ViewQuality` | `view = 0` | Supozohet pa view te vecante |
| `Renovated = false` ose mungon `YearRenovated` | `yr_renovated = 0` | 0 per modelin do te thote pa renovim |
| `NearbyLivingArea` | `sqft_living15 = sqft_living` | Perdor prone aktuale si fallback |
| `NearbyLotArea` | `sqft_lot15 = sqft_lot` | Perdor parcelen aktuale si fallback |

Kjo e ben formen me te perdorshme: perdoruesi mund te gjeneroje cmim edhe pa ditur cdo detaj teknik.

## Hapi 12: kerkesa i shkon ML service

Backend-i e dergon request-in me `HttpClient.PostAsJsonAsync`.

Endpoint-i merret nga konfigurimi:

```text
Ml:PredictUrl
```

Nese nuk eshte konfiguruar, default eshte:

```text
http://127.0.0.1:8000/predict
```

Ketu perfundon pjesa qe duhet shpjeguar per integrimin. Cfare ndodh brenda ML service, si eshte trajnuar modeli, cilat algoritme perdor, ose si e llogarit `predicted_price`, i takon repo-se se ML-se.

## Hapi 13: backend merr pergjigjen nga ML

Backend pret kete forme pergjigjeje:

```json
{
  "predicted_price": 464627.78,
  "predicted_price_formatted": "$464,628"
}
```

Pastaj backend e kthen kete ne format me te pershtatshem per frontend:

```json
{
  "suggestedPrice": 464627.78,
  "formattedPrice": "$464,628",
  "mlWarnings": []
}
```

`suggestedPrice` eshte numer dhe mund te vendoset ne input-in e cmimit.

`formattedPrice` eshte string i gatshem per shfaqje, p.sh. `$464,628`.

`mlWarnings` jane paralajmerime qe nuk e ndalin request-in, por i tregojne perdoruesit se input-i mund te jete jashte zones se trajnimit te modelit.

## Hapi 14: warnings qe nuk e ndalin request-in

Backend kthen warnings ne keto raste:

- Latitude/longitude jane jashte zones se trajnimit.
- Zipcode eshte jashte zones se trajnimit.
- Year built eshte jashte intervalit te trajnimit.

Rregullat ne kod:

```text
latitude duhet te jete afersisht brenda 47.1559 - 47.7776
longitude duhet te jete afersisht brenda -122.519 - -121.315
zipcode duhet te jete 98001 - 98199
yearBuilt duhet te jete 1900 - 2015 per te mos dhene warning
```

Keto nuk jane hard validation. Kerkesa vazhdon dhe ML mund te ktheje cmim, por UI shfaq paralajmerim.

Arsyeja: nje vlere mund te jete teknikisht valide, p.sh. latitude 46 eshte latitude reale, por mund te jete jashte zones ku modeli ka mesuar mire. Prandaj nuk e bllokojme domosdoshmerisht, por e paralajmerojme perdoruesin.

## Hapi 15: frontendi shfaq rezultatin

Kur backend kthen sukses:

- `generatedPrice` ruhet ne state
- `priceState` behet `success`
- UI shfaq `formattedPrice`, ose nese mungon, e formaton `suggestedPrice`
- butoni `Apply to Price` aktivizohet
- warnings shfaqen ne Generate Price card

Kur agjenti shtyp `Apply to Price`, thirret:

```text
applyGeneratedPrice()
```

Ky funksion vendos:

```text
form.price = generatedPrice.suggestedPrice.toFixed(2)
```

Pra cmimi i sugjeruar kopjohet ne fushen `Final Price`. Pas kesaj, perdoruesi ende mund ta ndryshoje manualisht.

## Rast 1: te gjitha te dhenat jane te plotesuara

Input shembull:

```text
Living Area: 1960 sqft
Bedrooms: 4
Bathrooms: 3
Floors: 1
Year Built: 1965
Latitude: 47.520801
Longitude: -122.393001
Zipcode: 98136
Lot Area: 5000 sqft
Condition: 5
Grade: 7
Has Basement: true
Basement Area: 910 sqft
Waterfront: false
View: 0
Renovated: false
Nearby Living: 1360 sqft
Nearby Lot: 5000 sqft
```

Cfare ndodh:

- Frontendi validon dhe lejon request-in.
- Backend validon prape.
- Backend konverton dhe mapon fushat.
- `sqft_above = 1960 - 910 = 1050`.
- `yr_renovated = 0`, sepse `renovated = false`.
- ML merr payload-in teknik.
- Backend merr `predicted_price`.
- UI shfaq cmimin dhe mundeson `Apply to Price`.

Ky eshte skenari ideal.

## Rast 2: mungojne te dhenat opsionale

Input shembull:

```text
Living Area: 1800 sqft
Bedrooms: 3
Bathrooms: 2
Floors: 1
Year Built: 2005
Latitude: 47.6062
Longitude: -122.3321
Zipcode: 98101
Lot Area: empty
Condition: empty/null
Grade: empty/null
Basement: false
Waterfront: false
Nearby fields: empty
```

Cfare ben backend:

```text
sqft_living = 1800
sqft_lot = 1800
condition = 3
grade = 7
sqft_basement = 0
sqft_above = 1800
waterfront = 0
view = 0
yr_renovated = 0
sqft_living15 = 1800
sqft_lot15 = 1800
sale_year = 2014
sale_month = 11
```

Pse pranohet:

Fushat opsionale e permiresojne saktesine, por nuk jane minimale per te ndertuar ML request. Backend vendos defaults per te krijuar payload komplet.

Si ta shpjegosh te profesori:

"Ne e dallojme mes fushave required dhe optional. Required duhen per te pasur nje prone minimale valide. Optional ndihmojne saktesine. Kur mungojne, backend-i vendos vlera neutrale/default qe jane te pranueshme per modelin."

## Rast 3: mungon nje fushe required

Shembull: mungon `Bedrooms`.

Cfare ndodh:

- `getMissingPriceFields` e liston `Bedrooms`.
- UI shfaq `Needed: Bedrooms`.
- `Generate Price` nuk duhet te jete i perdorshem.
- Nese dikush e anashkalon UI-ne dhe dergon request direkt, backend validon dhe kthen `400 Bad Request`.

Pse nuk pranohet:

ML contract kerkon numer dhomash. Default per bedrooms do te ishte spekulim i rrezikshem, sepse ndikon shume ne cmim.

## Rast 4: basement me i madh se living area

Input:

```text
Living Area: 50 m2
Has Basement: true
Basement Area: 60 m2
```

Cfare ndodh:

- Frontendi e kap kete rast kur basement area konvertohet dhe del me e madhe se living area.
- Backend e validon prape.
- Backend kthen validation error per `BasementArea`.
- Kerkesa nuk i shkon ML.

Pse nuk pranohet:

`sqft_above = sqft_living - sqft_basement`. Nese basement eshte me i madh se living area, `sqft_above` del negativ. Kjo nuk ka kuptim fizik per pronen dhe do te prishte input-in e modelit.

## Rast 5: prone e renovuar pa vit renovimi

Input:

```text
Renovated: true
Year Renovated: empty
```

Cfare ndodh:

- Frontendi kerkon `yearRenovated`.
- Backend po ashtu kerkon `YearRenovated` kur `Renovated = true`.
- Nese mungon, kthehet `400 Bad Request`.

Pse:

Per ML, renovimi perfaqesohet me `yr_renovated`. Nese perdoruesi thote qe prona eshte renovuar, duhet te dihet viti. Pa vit, backend nuk mund ta vendose sakte kete fushe.

## Rast 6: renovated false dhe yearRenovated bosh

Input:

```text
Renovated: false
Year Renovated: empty
```

Cfare ndodh:

- Kjo pranohet.
- Backend vendos `yr_renovated = 0`.

Pse:

Ne kete kontrate, `0` perdoret si sinjal se nuk ka renovim.

## Rast 7: koordinata valide, por jashte zones se modelit

Input:

```text
Latitude: 46.0
Longitude: -122.0
Zipcode: 98101
```

Cfare ndodh:

- Latitude 46 eshte gjeografikisht valide, sepse eshte mes -90 dhe 90.
- Backend nuk e bllokon per kete arsye.
- Por backend shton warning: `Location is outside the model training area.`
- ML request mund te vazhdoje.
- UI shfaq warning afer cmimit.

Pse eshte warning dhe jo error:

Sepse koordinata eshte reale, por mund te jete jashte zones ku modeli ka te dhena te forta trajnimi. Sistemi e lejon, por e ben perdoruesin te vetedijshem qe parashikimi mund te jete me pak i besueshem.

## Rast 8: zipcode jashte zones se modelit

Input:

```text
Zipcode: 10001
```

Cfare ndodh:

- Backend e pranon sepse eshte numer pozitiv.
- Backend shton warning: `Zipcode is outside the model training area.`

Pse:

Zipcode 10001 eshte teknikisht valid si numer, por modeli eshte trajnuar per intervalin 98001-98199. Prandaj nuk eshte error teknik, por eshte sinjal per risk ne saktesi.

## Rast 9: yearBuilt valid per sistemin, por jashte training range

Input:

```text
Year Built: 2020
```

Cfare ndodh:

- Backend e pranon nese eshte <= currentYear + 2.
- Backend shton warning: `Year built is outside the model training range.`

Pse:

Sistemi lejon prona te reja, sepse ne biznes mund te ekzistojne. Por modeli mund te jete trajnuar ne interval ku viti 2020 nuk ishte i mbuluar mire. Prandaj warning.

## Rast 10: ML service nuk eshte ndezur

Cfare ndodh:

- Backend tenton te dergoje request ne `http://127.0.0.1:8000/predict`.
- Nese ML service nuk pergjigjet, `HttpClient` deshton.
- Backend e kap gabimin dhe kthen `503 Service Unavailable`.
- Frontendi shfaq mesazh gabimi ne Generate Price card.

Pse perdoret `503`:

`503 Service Unavailable` do te thote qe backend-i eshte gjalle, por nje service i varur prej tij nuk eshte i disponueshem. Ketu service i varur eshte ML prediction service.

## Rast 11: perdoruesi nuk ka permission

Cfare ndodh:

- Endpoint-i ka `[Authorize(Policy = Permissions.CreateProperty)]`.
- Nese perdoruesi nuk eshte i kycur, backend kthen `401`.
- Nese eshte i kycur por nuk ka permission, backend kthen `403`.
- `api.ts` i kthen keto ne mesazhe te lexueshme per UI.

Pse:

Generate Price eshte pjese e workflow-t te krijimit te prones, prandaj lidhet me permission `CreateProperty`.

## Rast 12: perdoruesi ndryshon fushat pasi gjeneron cmim

Ne `updateFormField`, nese ndryshohet nje fushe tjeter pervec `price`, frontendi:

- fshin `generatedPrice`
- kthen `priceState` ne `idle`
- fshin `priceMessage`

Pse:

Cmimi i gjeneruar lidhet me input-et e momentit kur u dergua request-i. Nese perdoruesi ndryshon living area, bedrooms ose lokacionin, cmimi i vjeter nuk eshte me i besueshem per formen e re.

## Error handling

Ka disa nivele gabimesh:

| Ku ndodh | Shembull | Rezultat |
| --- | --- | --- |
| Frontend validation | mungon bedrooms | UI shfaq gabim dhe nuk dergon request |
| Backend validation | request direkt me invalid data | `400 Bad Request` |
| Authorization | user pa permission | `401` ose `403` |
| ML unavailable | ML service down | `503 Service Unavailable` |
| ML response bosh | ML kthen empty response | backend e trajton si failure |
| Network error frontend-backend | backend nuk arrihet | UI shfaq network error |

## Pikat kryesore qe duhet t'i thuash ne prezantim

1. Generate Price eshte backend-mediated ML integration.
2. Frontendi nuk komunikon direkt me ML service.
3. Frontendi ben validim per UX, backend ben validim per siguri dhe integritet.
4. Backend e perkthen formen agent-friendly ne kontraten teknike te ML-se.
5. Te gjitha siperfaqet normalizohen ne square feet.
6. Optional fields nuk e bllokojne request-in; backend vendos defaults.
7. Invalid fields bllokohen para se kerkesa te shkoje ne ML.
8. Warnings jane ndryshe nga errors: warnings tregojne risk saktesie, errors bllokojne request-in.
9. ML kthen `predicted_price`; backend e kthen ne `suggestedPrice` per frontend.
10. Cmimi i gjeneruar eshte sugjerim dhe agjenti mund ta ndryshoje.

## Pyetje te mundshme nga profesori

### Pse nuk i dergoni te gjitha fushat e formes ne ML?

Sepse jo te gjitha fushat jane relevante per modelin. `title`, `description`, `companyId`, `agentId`, `propertyStatusId` jane te rendesishme per menaxhimin e listing-ut, por jo per kete kontrate te ML-se. Backend dergon vetem fushat qe ML contract kerkon.

### Pse validoni edhe ne frontend edhe ne backend?

Frontend validation eshte per experience me te mire: perdoruesi merr gabim menjehere. Backend validation eshte per siguri dhe saktesi, sepse request-et mund te vijne edhe pa UI.

### Pse disa raste jane warning dhe jo error?

Sepse input-i mund te jete teknikisht valid, por jashte zones ku modeli eshte me i besueshem. Shembull: zipcode pozitiv por jashte intervalit te trajnimit. Nuk eshte gabim formatimi, por eshte risk per saktesi.

### Pse optional fields kane default?

Sepse modelit i duhet payload komplet, por perdoruesi mund te mos i dije te gjitha detajet. Defaults si `condition = 3`, `grade = 7`, `waterfront = 0` jane vlera neutrale qe lejojne sistemin te funksionoje pa e bere formen shume te rende.

### Pse basement nuk mund te jete me i madh se living area?

Sepse backend llogarit `sqft_above = sqft_living - sqft_basement`. Nese basement eshte me i madh, rezultati del negativ dhe nuk ka kuptim per modelin.

### Pse `yr_renovated` behet 0 kur nuk ka renovim?

Sepse kontrata e ML-se pret numer. Ne kete flow, `0` perdoret si vlere qe tregon se prona nuk eshte renovuar.

### Pse perdoret `CreateProperty` permission per Generate Price?

Sepse Generate Price eshte pjese e krijimit te listing-ut. Vetem perdoruesit qe kane te drejte te krijojne prona duhet te mund te perdorin kete endpoint.

### Cfare ndodh nese ML service deshton?

Backend kthen `503 Service Unavailable`, ndersa frontendi e shfaq si gabim ne card. Aplikacioni nuk bie komplet; vetem price generation nuk funksionon per momentin.

### A ruhet cmimi i gjeneruar automatikisht ne databaze?

Jo. Generate Price vetem sugjeron cmim. Cmimi ruhet vetem nese perdoruesi e aplikon ose e shkruan ne `Final Price` dhe pastaj krijon property me submit.

### Pse backend logon request dhe response te ML?

Per debugging dhe observability. Nese prediksioni del gabim ose ML service deshton, log-et ndihmojne te shihet cfare payload-i u dergua dhe cfare u kthye.

## File-t kryesore ne kod

Frontend:

- `frontend/src/pages/CreatePropertyPage.tsx`
- `frontend/src/services/api.ts`
- `frontend/vite.config.ts`

Backend:

- `backend/EstateIQ/Controllers/PropertiesController.cs`
- `backend/EstateIQ/DTOs/GeneratePropertyPriceRequestDto.cs`
- `backend/EstateIQ/DTOs/GeneratePropertyPriceResponseDto.cs`
- `backend/EstateIQ/Interfaces/IPropertyPricePredictionService.cs`
- `backend/EstateIQ/Services/PropertyPricePredictionService.cs`
- `backend/EstateIQ/Program.cs`

Tests:

- `backend/EstateIQ.Tests/PropertyPricePredictionServiceTests.cs`

## Permbledhje e shkurter per ta thene me goje

Kur agjenti shtyp `Generate Price`, frontendi kontrollon qe fushat minimale si living area, bedrooms, bathrooms, floors, year built, latitude, longitude dhe zipcode jane valide. Pastaj nderton nje payload agent-friendly dhe e dergon te backend-i ne `POST /api/properties/generate-price`.

Backend-i kontrollon permission `CreateProperty`, validon request-in prape, konverton siperfaqet ne square feet, mbush fushat opsionale me defaults dhe nderton payload-in teknik qe ML service e kerkon. Pastaj backend-i e dergon request-in te ML endpoint. Kur ML kthen `predicted_price`, backend-i e kthen si `suggestedPrice` dhe `formattedPrice`, plus warnings nese input-i eshte jashte zones se trajnimit. Frontendi e shfaq cmimin si sugjerim dhe agjenti mund ta aplikoje ose ta ndryshoje manualisht.
