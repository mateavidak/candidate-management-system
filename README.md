# Candidate Management System

.NET 8 Web API (PostgreSQL + EF Core) za CRUD kandidata i veština, REST endpointe, seed podataka i xUnit testove. React klijent nalazi se u folderu `client/`.

## Pokretanje

### Backend

1. Podesiti PostgreSQL i connection string u `src/CandidateManagementSystem.Api/appsettings.Development.json`
2. Pokrenuti aplikaciju komandom `dotnet run` iz foldera `src/CandidateManagementSystem.Api`

### Frontend

1. Otvoriti `client/` folder
2. Pokrenuti komande:

```bash id="b9021"}
npm install
npm run dev
```

## Pokretanje testova

```bash id="b9022"}
dotnet test
```

## Glavni endpointi

* `GET` `/api/candidates`
* `GET` `/api/candidates/search`
* `POST` `/api/candidates`
* `PUT` `/api/candidates/{id}`
* `DELETE` `/api/candidates/{id}`
* `POST` `/api/candidates/{id}/skills/{skillId}`
* `DELETE` `/api/candidates/{id}/skills/{skillId}`
* `GET` `/api/skills`
* `POST` `/api/skills`
* `DELETE` `/api/skills/{id}`

## Najzahtevniji / najzanimljiviji deo zadatka

Najzahtevniji deo zadatka bio je **pretraga kandidata po imenu i po više veština**, uz očuvanje konzistentnih pravila u bazi i predvidivog ponašanja API-ja.

Više veština u jednom upitu tretira se kao **AND** uslov — kandidat mora imati sve tražene veštine. Ovo odgovara realnom HR scenariju pretrage, npr. kandidat koji zna **Java i SQL**, a ne samo jednu od tih tehnologija.

U `CandidateService.SearchAsync` prikupljaju se obavezni `skillId` parametri direktno iz upita i preko mapiranja naziva veština iz tabele `Skills`. Ako neko ime veštine ne postoji, rezultat je prazan skup, čime se izbegavaju delimični i neprecizni rezultati.

Za pretragu po imenu korišćen je pristup **`ToLower()` + `Contains()`** umesto `EF.Functions.ILike`, kako bi ista logika radila i na PostgreSQL bazi i u **InMemory** testovima, gde `ILike` nije podržan.

Integritet podataka obezbeđen je korišćenjem `citext` tipa, jedinstvenih indeksa i `check` ograničenja (`btrim`) za sprečavanje duplikata i nevalidnih unosa. Konflikti poput duplog email-a ili postojeće veštine obrađuju se kroz `ApiConflictException` i `DbUpdateException`, umesto generičkog HTTP 500 odgovora.

Najveći izazov bio je uskladiti **ispravnu semantiku pretrage**, **portabilnost EF upita** i **jasno rukovanje konfliktima**, kako bi sistem bio pouzdan i koristan i za HR korisnike i za dalji razvoj i testiranje.
