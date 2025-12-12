# AntiPlagiarism-KPO-DZ3

Учебный проект по КПО: микросервисное приложение для проверки студенческих работ на плагиат.

Архитектура состоит из трёх REST-микросервисов:

- **File Storage** – хранение файлов работ.
- **Analysis** – анализ похожести работ, хранение `Submission` и `Report`.
- **Gateway** – фасад для внешних клиентов (студент / преподаватель).

Для упрощения все данные хранятся **в памяти** (in-memory репозитории), без реальной БД.

---

## 1. Структура репозитория

В корне репо:

- `AntiPlagiarism.sln` – solution Visual Studio (.NET 8).
- **Проекты:**
  - `AntiPlagiarism` – File Storage API  
    (Web API, хранение метаданных файлов).
  - `AntiPlagiarism.Analysis.Api` – Analysis API  
    (сабмишены, отчёты, логика плагиата).
  - `AntiPlagiarism.Gateway.Api` – Gateway API  
    (единая точка входа для клиента).
- `README.md` – этот файл.
- `.gitignore` – исключает `bin/`, `obj/`, `.vs` и прочий мусор.

---

## 2. Используемый стек

- **.NET 8**, ASP.NET Core Minimal APIs
- HttpClient для общения между сервисами
- Swashbuckle (Swagger) для документации
- In-memory репозитории (без внешней БД)

---

## 3. Запуск проекта

### 3.1. Через Visual Studio 2022

1. Открыть `AntiPlagiarism.sln`.
2. Правый клик по решению → **Свойства** →  
   **Настроить запускаемые проекты**:
   - выбрать **Несколько запускаемых проектов**;
   - для всех трёх проектов (`AntiPlagiarism`, `AntiPlagiarism.Analysis.Api`, `AntiPlagiarism.Gateway.Api`) установить действие **Запустить**.
3. Нажать **ОК** и запустить решение (F5 / Ctrl+F5).

По умолчанию сервисы слушают порты (могут отличаться у тебя, смотри в консоль):

- File Storage: `http://localhost:5110`
- Analysis: `http://localhost:5173`
- Gateway: `http://localhost:5053`

### 3.2. Через `dotnet run`

В трёх терминалах:

```bash
cd AntiPlagiarism
dotnet run

cd AntiPlagiarism.Analysis.Api
dotnet run

cd AntiPlagiarism.Gateway.Api
dotnet run
4. Swagger-интерфейсы
У каждого сервиса есть Swagger UI:

File Storage: http://localhost:5110/swagger

Analysis: http://localhost:5173/swagger

Gateway: http://localhost:5053/swagger

Основная точка входа для клиента — Gateway Swagger.

5. Gateway API (основные эндпоинты)
Базовый адрес: http://localhost:5053

5.1. POST /api/works/{workId}/submit
Отправка студентом работы на проверку.

Параметры:

workId (route) — идентификатор работы, например "control-1".

Тело запроса: multipart/form-data

file – файл работы (IFormFile).

studentId – идентификатор студента (например, "s1").

fileKind – тип файла; опционально, по умолчанию "Work".

Ответ 200 OK – SubmissionAnalysisResponseDto:

json
Копировать код
{
  "submissionId": "GUID",
  "workId": "control-1",
  "studentId": "s1",
  "fileId": "GUID",
  "status": "Completed",
  "reportId": "GUID",
  "isPlagiarism": false,
  "similarityPercent": 0,
  "baseSubmissionId": null,
  "createdAtUtc": "2025-12-12T01:11:28Z"
}
За кадром Gateway:

Шлёт файл в File Storage (POST /files).

Получает fileId.

Отправляет запрос в Analysis (POST /submissions).

Возвращает клиенту агрегированный результат.

5.2. GET /api/reports/{reportId}
Получение одного отчёта по идентификатору.

Параметры:

reportId – GUID отчёта.

Ответ 200 OK – ReportResponseDto (данные из Analysis):

json
Копировать код
{
  "reportId": "GUID",
  "submissionId": "GUID",
  "workId": "control-1",
  "studentId": "s1",
  "fileId": "GUID",
  "isPlagiarism": false,
  "similarityPercent": 0,
  "baseSubmissionId": null,
  "createdAtUtc": "2025-12-12T01:11:28Z",
  "submittedAtUtc": "2025-12-12T01:11:28Z"
}
5.3. GET /api/works/{workId}/reports
Получение всех отчётов по конкретной работе.

Параметры:

workId – идентификатор работы, например "control-1".

Ответ 200 OK – WorkReportsResponseDto:

json
Копировать код
{
  "workId": "control-1",
  "reports": [
    {
      "reportId": "GUID",
      "studentId": "s1",
      "isPlagiarism": false,
      "similarityPercent": 0,
      "baseSubmissionId": null,
      ...
    },
    {
      "reportId": "GUID",
      "studentId": "s2",
      "isPlagiarism": true,
      "similarityPercent": 100,
      "baseSubmissionId": "GUID первой сдачи",
      ...
    }
  ]
}
6. File Storage API (кратко)
База: http://localhost:5110

Основные эндпоинты:

POST /files

multipart/form-data (file, fileKind).

Возвращает fileId и метаданные (UploadFileResponseDto).

GET /files/{id}/metadata

Возвращает StoredFileMetadata по fileId.

Реализация хранения упрощённая: данные хранятся в in-memory репозитории.

7. Analysis API (кратко)
База: http://localhost:5173

Основные эндпоинты:

POST /submissions

Принимает CreateSubmissionRequestDto:

workId, studentId, fileId, submittedAtUtc.

Создаёт Submission и Report.

Определяет плагиат среди уже существующих сабмишенов той же работы.

GET /reports/{reportId}

Возвращает конкретный отчёт.

GET /works/{workId}/reports

Возвращает список отчётов по работе.

Алгоритм проверки плагиата упрощён: если контент файлов полностью совпадает, считаем similarityPercent = 100 и помечаем работу как плагиат (isPlagiarism = true, baseSubmissionId указывает на исходную сдачу).

8. Пример сценария демонстрации
Шаг 1. Студент s1 сдаёт работу
Запустить все три сервиса.

Открыть Swagger Gateway: http://localhost:5053/swagger.

В POST /api/works/{workId}/submit:

workId = control-1;

studentId = s1;

приложить файл test1.txt.

Ответ: isPlagiarism = false, similarityPercent = 0.

Шаг 2. Студент s2 сдаёт такой же файл
Повторить запрос:

workId = control-1;

studentId = s2;

тот же test1.txt.

Ответ: isPlagiarism = true, similarityPercent = 100,
baseSubmissionId указывает на сдачу s1.

Шаг 3. Просмотр отчётов преподавателем
GET /api/works/control-1/reports
→ видно две записи: s1 (оригинал) и s2 (плагиат).

GET /api/reports/{reportId} для интересующего отчёта
→ детальная информация по конкретной сдаче.

9. Упрощения
Все данные (Submission, Report, метаданные файлов) живут в памяти процесса.

Содержимое файлов может храниться в минимальном виде; цель — показать интеграцию сервисов.

Алгоритм плагиата максимально простой (полное совпадение → 100%).

10. Статус соответствия ТЗ (на 9 баллов)
Реализовано:

Микросервисная архитектура: File Storage, Analysis, Gateway.

In-memory репозитории для сабмишенов и отчётов.

Загрузка файлов и сохранение метаданных.

Создание сабмишенов и отчётов, маркировка плагиата.

Эндпоинты Gateway:

POST /api/works/{workId}/submit

GET /api/reports/{reportId}

GET /api/works/{workId}/reports

Swagger-документация у всех сервисов.

Рабочий сценарий: оригинальная сдача + плагиат + просмотр отчётов.

Копировать код

Если хочешь, могу дополнительно написать маленький раздел в README типа «Как запускать через Docker», но для ДЗ и защиты, скорее всего, этого уже достаточно.





