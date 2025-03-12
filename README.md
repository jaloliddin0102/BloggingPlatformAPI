# Blogging Platform API

## Umumiy Ma'lumot

**Blogging Platform API** – bu to‘liq funksiyali blog platformasi uchun mo‘ljallangan backend API bo‘lib, foydalanuvchilarga ro‘yxatdan o‘tish, blog postlar yaratish, postlarga komment qoldirish, like qo‘yish, media fayllarni qo‘shish va real vaqt rejimida bildirishnomalar olish imkoniyatini beradi. API **JWT** orqali autentifikatsiya qilinadi va **role-based access control** (rol asosidagi kirishni boshqarish) ni qo‘llaydi.

## Loyihaning Maqsadi

Ushbu loyiha quyidagi asosiy maqsadlarni ko‘zda tutadi:
- **Foydalanuvchi boshqaruvi:** Foydalanuvchilar ro‘yxatdan o‘tishi va tizimga kirishi, shuningdek, ro‘l asosida (admin, author, moderator, user) huquqlarni boshqarish.
- **Post boshqaruvi:** Blog postlarini yaratish, o‘zgartirish, o‘chirish va ko‘rish; postlarni draft (qoralama) yoki published (nashr etilgan) holatda saqlash.
- **Muloqot:** Postlarga komment qo‘shish va like bosish imkoniyati.
- **Media qo‘shish:** Postlarga rasm, video va boshqa turdagi fayllarni qo‘shish.
- **Real-Vaqt Bildirishnomalari:** SignalR yordamida foydalanuvchilarga real vaqt rejimida bildirishnomalar yuborish.
- **Kengaytirilgan Qidiruv va Filtrlash:** Postlarni holat, kategoriya, teg va kalit so‘zlar bo‘yicha qidirish va filtrlash.

## Foydalanilgan Texnologiyalar

- **.NET 8**  
- **ASP.NET Core Web API**  
- **Entity Framework Core**  
- **PostgreSQL** (ma'lumotlar bazasi)  
- **JWT Authentication**  
- **SignalR** (real vaqt bildirishnomalari uchun)  
- **Swagger** (API hujjatlari uchun)  
- **FluentValidation** (model validatsiyasi, agar qo‘llanilsa)

## O‘rnatish va Sozlash

### 1. Loyihani Klonlash
```bash
git clone https://github.com/username/BloggingPlatformAPI.git
cd BloggingPlatformAPI
```

### 2. `appsettings.json` Faylini Sozlash

`appsettings.json` faylida ma'lumotlar bazasi ulanishi va JWT konfiguratsiyasi mavjud. Misol uchun:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=BloggingPlatformDb;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "SecretKey": "SuperSecretKeyForJwtTokenGeneration",
    "Issuer": "BloggingPlatform",
    "Audience": "BloggingPlatformUsers",
    "ExpirationMinutes": 120
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```
> **Eslatma:** `SecretKey` ni kamida 32 belgidan iborat kuchli kalit bilan almashtiring.

### 3. Migratsiyalarni Yaratish va Ma'lumotlar Bazasini Yangilash
Terminal yoki Package Manager Console orqali quyidagilarni bajaring:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Loyihani Ishga Tushurish
```bash
dotnet run
```
Server `http://localhost:5110` (yoki konfiguratsiyadagi port) manzilida ishlaydi.

### 5. Swagger UI orqali API ni Sinash
Brauzeringizda quyidagi manzilga o‘ting:
```
http://localhost:5110/swagger/index.html
```
Bu yerda barcha endpointlar hujjatlashtirilgan va sinash mumkin.

## API Endpointlari

### Authentication
- **Ro‘yxatdan O‘tish**  
  **Endpoint:** `POST /api/auth/register`  
  **Request Body:**
  ```json
  {
    "username": "testuser",
    "password": "Secure123!",
    "role": "author"
  }
  ```
  **Javoblar:**  
  - 201 Created  
  - 400 Bad Request (validatsiya xatolari)

- **Login va JWT Token Olish**  
  **Endpoint:** `POST /api/auth/login`  
  **Request Body:**
  ```json
  {
    "username": "testuser",
    "password": "Secure123!"
  }
  ```
  **Javoblar:**  
  - 200 OK (JWT token)  
  - 401 Unauthorized

### Post Management
- **Post Yaratish**  
  **Endpoint:** `POST /api/posts`  
  **Authorization:** Admin yoki Author  
  **Request Body:**
  ```json
  {
    "title": "My First Blog Post",
    "content": "This is the content of the blog post.",
    "status": "Draft",
    "publishedAt": null,
    "categoryId": "GUID-of-category",
    "tagIds": ["GUID-of-tag1", "GUID-of-tag2"]
  }
  ```
  **Javoblar:**  
  - 201 Created  
  - 400 Bad Request

- **Postni Ko‘rish (ID bo‘yicha)**  
  **Endpoint:** `GET /api/posts/{id}`  
  **Javoblar:**  
  - 200 OK  
  - 404 Not Found

- **Postni Yangilash**  
  **Endpoint:** `PUT /api/posts/{id}`  
  **Authorization:** Admin yoki Author  
  **Javoblar:**  
  - 204 No Content  
  - 403 Forbidden (agar ruxsat yo‘q bo‘lsa)  
  - 404 Not Found

- **Postni O‘chirish**  
  **Endpoint:** `DELETE /api/posts/{id}`  
  **Authorization:** Admin yoki Author  
  **Javoblar:**  
  - 204 No Content  
  - 403 Forbidden  
  - 404 Not Found

### Interactions
- **Postga Like Qo‘yish**  
  **Endpoint:** `POST /api/posts/{postId}/like`  
  **Authorization:** Admin, Author, Moderator, User  
  **Javoblar:**  
  - 200 OK  
  - 400 Bad Request

- **Like-ni Olib Tashlash**  
  **Endpoint:** `DELETE /api/posts/{postId}/like`  
  **Authorization:** Admin, Author, Moderator, User  
  **Javoblar:**  
  - 204 No Content  
  - 400 Bad Request

- **Postga Komment Qo‘shish**  
  **Endpoint:** `POST /api/posts/{postId}/comments`  
  **Authorization:** Admin, Author, Moderator, User  
  **Request Body:**
  ```json
  {
    "content": "Great post!"
  }
  ```
  **Javoblar:**  
  - 201 Created  
  - 400 Bad Request

- **Postga Media Fayl Yuklash**  
  **Endpoint:** `POST /api/posts/{postId}/media`  
  **Authorization:** Admin yoki Author  
  **Request Type:** `multipart/form-data`  
  **Parameters:**  
    - File (IFormFile)  
    - fileType (e.g., "image", "video")  
  **Javoblar:**  
  - 201 Created  
  - 400 Bad Request

### Notifications
- **Bildirishnomalarni Olish**  
  **Endpoint:** `GET /api/notifications`  
  **Authorization:** Admin, Author, Moderator, User  
  **Javoblar:**  
  - 200 OK  
  - 401 Unauthorized

- **Bildirishnomani "Read" Qilish**  
  **Endpoint:** `PUT /api/notifications/{id}/read`  
  **Authorization:** Admin, Author, Moderator, User  
  **Javoblar:**  
  - 204 No Content  
  - 404 Not Found

### Real-Time Notifications (SignalR)
The API uses **SignalR** to send real-time notifications.  
**Hub Endpoint:**  
```
http://localhost:5110/notificationsHub
```
**Client Example (C#):**
```csharp
using Microsoft.AspNetCore.SignalR.Client;

class Program
{
    static async Task Main()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5110/notificationsHub")
            .Build();

        connection.On<string>("ReceiveNotification", message =>
        {
            Console.WriteLine($"New notification: {message}");
        });

        await connection.StartAsync();
        Console.WriteLine("Connected to SignalR hub.");
        Console.ReadLine();
    }
}
```

## How to Contribute

Contributions, suggestions, and improvements are welcome!  
1. Fork the repository.  
2. Create your feature branch (`git checkout -b feature/YourFeature`).  
3. Commit your changes (`git commit -m 'Add some feature'`).  
4. Push to the branch (`git push origin feature/YourFeature`).  
5. Open a Pull Request.

## License

This project is licensed under the MIT License.

---

**Ready for deployment and further extension!**  
Agar qo‘shimcha savollaringiz bo‘lsa yoki yordam kerak bo‘lsa, iltimos, murojaat qiling.

---
