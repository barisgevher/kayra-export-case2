# ProductManagement Projesi case 2

## Giriş

Bu proje, temel backend geliştirme prensiplerini, REST API oluşturma ve veritabanı işlemlerini ölçmek amacıyla geliştirilmiş bir "case study" projesidir. Uygulama, C# ve ASP.NET Core kullanılarak geliştirilmiş olup, veri depolama için PostgreSQL veritabanını ve önbellekleme/oturum yönetimi için Redis'i kullanmaktadır. Tüm servisler Docker ve Docker Compose ile yönetilmektedir.

## Kullanılan Teknolojiler

- **Backend:** ASP.NET Core (C#)
- **Veritabanı:** PostgreSQL 15
- **Önbellek/Oturum:** Redis 7-alpine
- **Veritabanı Yönetimi:** pgAdmin 4
- **Konteynerleştirme:** Docker, Docker Compose
- **ORM:** Entity Framework Core (API projesi içinde)

## Proje Bileşenleri

Bu proje, Docker Compose ile birlikte aşağıdaki servisleri çalıştırmaktadır:

- **`productmanagement-api`**: Ana ASP.NET Core API uygulaması. HTTP isteklerini dinler ve iş mantığını yürütür.
- **`postgres`**: Projenin birincil veri depolaması için kullanılan PostgreSQL veritabanı sunucusu.
- **`redis`**: API tarafından önbellekleme veya oturum yönetimi için kullanılan Redis sunucusu.
- **`pgadmin`**: PostgreSQL veritabanını web arayüzü üzerinden yönetmek için kullanılan bir yönetim aracı.

## Kurulum ve Çalıştırma Adımları

Projenin yerel makinenizde nasıl kurulacağını ve çalıştırılacağını gösteren adımlar aşağıdadır.

### Ön Koşullar

Projenin doğru şekilde çalışabilmesi için sisteminizde aşağıdaki yazılımların yüklü olması gerekmektedir:

- **Git:** Proje kodlarını klonlamak için.
- **Docker Desktop (veya Docker Engine & Docker Compose):** Tüm servisleri konteynerize etmek ve yönetmek için.

### Kurulum Adımları

1.  **Depoyu Klonlayın:**
    Öncelikle projenin kodlarını yerel makinenize klonlayın:

    ```bash
    git clone https://github.com/barisgevher/kayra-export-case2.git
    cd kayra-export-case2
    # Eğer API projesi ana dizinde değilse, docker-compose.yml'nin bulunduğu dizine gidin.
    # Örneğin: cd ProductManagement
    ```

    _Not: Yukarıdaki `cd` komutu, `docker-compose.yml` dosyanızın bulunduğu dizine gitmenizi sağlar. Eğer `kayra-export-case2` içinde başka bir `ProductManagement` klasörü varsa, o klasöre girmeniz gerekebilir._

2.  **`.env` Dosyasını Oluşturun:**
    `docker-compose.yml` dosyasının bulunduğu dizinde `.env` adında yeni bir dosya oluşturun ve içine aşağıdaki satırları ekleyin. Bu değişkenleri kendi istediğiniz değerlerle güncelleyin:

    ```plaintext
    POSTGRES_DB=product_db
    POSTGRES_USER=product_user
    POSTGRES_PASSWORD=your_secure_postgres_password
    REDIS_PASSWORD=your_secure_redis_password
    JWT_SECRET=your_super_secret_jwt_key_that_is_long_and_complex
    PGADMIN_EMAIL=admin@example.com
    PGADMIN_PASSWORD=your_pgadmin_password
    ```

    _Güvenlik nedeniyle, `\_secure_` ile belirtilen şifreleri güçlü ve tahmin edilemez yapmanız önerilir.\_

3.  **Docker Konteynerlerini Başlatın:**
    `docker-compose.yml` ve `.env` dosyalarının bulunduğu dizinde aşağıdaki komutu çalıştırarak tüm servisleri oluşturun ve başlatın:

    ```bash
    docker-compose up --build -d
    ```

    - `--build`: Değişiklikler varsa API uygulamasının Docker görüntüsünü yeniden oluşturur.
    - `-d`: Servisleri arka planda (detached mode) başlatır.

4.  **Veritabanı Migrasyonlarını Uygulayın (Önemli!):**
    API projesi Entity Framework Core kullandığı için, veritabanı şemasının oluşturulması veya güncellenmesi gereklidir. Bu genellikle aşağıdaki yollarla yapılır:
    - **Otomatik:** Eğer API uygulamanızın başlangıç kodu, veritabanı bağlantısı kurulur kurulmaz migrasyonları otomatik olarak uyguluyorsa (örneğin `context.Database.Migrate()` çağrısı ile), ek bir adım gerekmez.
    - **Manuel (Tavsiye Edilen):** Eğer otomatik olarak uygulanmıyorsa, `productmanagement-api` konteyneri içinde manuel olarak migrasyonları çalıştırabilirsiniz. Öncelikle API konteynerinin adını bulun (genellikle `productmanagement_api` veya benzeri):
      ```bash
      docker ps
      ```
      Ardından aşağıdaki komutu kullanarak migrasyonları çalıştırın (API projenizin `csproj` dosyasının bulunduğu dizine göre yolu ayarlamanız gerekebilir):
      ```bash
      docker exec productmanagement_api dotnet ef database update --project productManagement.csproj --startup-project productManagement.csproj
    
   
      ```
      Bu komutu çalıştırmadan önce `productmanagement-api` konteynerinin tamamen başlamış olduğundan emin olun.

### Uygulamaya Erişim

Servisler başarıyla başlatıldıktan sonra aşağıdaki adreslerden erişebilirsiniz:

- **ProductManagement API:** `http://localhost:8000`
  - API uç noktaları genellikle `/api/v1/...` gibi bir önekle başlar. Swagger/OpenAPI UI'a genellikle `http://localhost:8000/swagger` adresinden erişebilirsiniz.
- **pgAdmin 4:** `http://localhost:8081`
  - Giriş için `.env` dosyasında belirlediğiniz `PGADMIN_EMAIL` ve `PGADMIN_PASSWORD` değerlerini kullanın.
  - pgAdmin'e giriş yaptıktan sonra, `productmanagement_postgres` servisine bağlanmak için yeni bir sunucu eklemeniz gerekecektir:
    - **Host name/address:** `postgres` (Bu, Docker ağındaki servis adıdır)
    - **Port:** `5432`
    - **Maintenance database:** `product_db` (veya `.env` dosyasındaki `POSTGRES_DB` değeriniz)
    - **Username:** `product_user` (veya `.env` dosyasındaki `POSTGRES_USER` değeriniz)
    - **Password:** `.env` dosyasındaki `POSTGRES_PASSWORD` değeriniz

### Servisleri Durdurma

Tüm Docker konteynerlerini durdurmak ve kaldırmak için proje dizininde aşağıdaki komutu kullanın:

```bash
docker-compose down
Eğer postgres_data ve redis_data volümlerini de kaldırmak isterseniz (veritabanı verilerini siler), aşağıdaki komutu kullanın:
code
Bash
docker-compose down -v
API Uç Noktaları
Uygulamanın tam API dokümantasyonu, genellikle /swagger adresindeki Swagger UI üzerinden erişilebilir.
GET /api/Products: Tüm ürünleri listeler.
GET /api/products/{id}: Belirli bir ID'ye sahip ürünü getirir.
POST /api/Products: Yeni bir ürün oluşturur.
PUT /api/Products/{id}: Belirli bir ID'ye sahip ürünü günceller.
DELETE /api/Products/{id}: Belirli bir ID'ye sahip ürünü siler.
POST /api/Auth/register: Yeni bir kullanıcı kayıt eder
POST /api/Auth/login: Kullanıcı girişini sağlar
```
