# Как сделать меню со списком пациентов и сессиями

Краткая пошаговая настройка UI в Unity для скрипта **HospitalPatientsListUI** (поиск по первым буквам, список пациентов, при клике — сессии).

---

## 1. Создать Canvas

1. В **Hierarchy** правый клик → **UI** → **Canvas**.
2. Появится **Canvas** и **EventSystem** (если не было). EventSystem не трогай.
3. Выбери **Canvas** → в **Inspector** проверь:
   - **Render Mode**: Screen Space - Overlay (или Camera, если используешь камеру для UI).
   - **UI Scale Mode**: Scale With Screen Size.
   - **Reference Resolution**: например 1920×1080, **Match**: 0.5.

---

## 2. Создать панель меню

1. Правый клик по **Canvas** → **UI** → **Panel**. Так появится панель (фон).
2. Выбери **Panel**:
   - **Rect Transform**: можно растянуть на весь экран (Anchor Preset: stretch-stretch, Left/Right/Top/Bottom = 0) или оставить по центру и задать Width/Height (например 500×600).
   - **Image (Script)** → Color: полупрозрачный тёмный (например R:0.1, G:0.1, B:0.15, A:200).

Эту панель можно переименовать в **PatientsMenuPanel**.

---

## 3. Поле поиска (InputField)

1. Правый клик по **Panel** → **UI** → **Input Field**.
2. Переименуй в **SearchField**.
3. **Rect Transform**:
   - Anchor: верх по центру (Top Center).
   - Pos Y: например -30, Width: 400, Height: 40.
4. **Input Field (Script)**:
   - **Placeholder** — дочерний объект с текстом типа "Nom ou prénom...".
   - **Text** — дочерний объект, куда пишется ввод (шрифт, размер — по вкусу).

Потом в скрипте мы привяжем этот объект к полю **Search Field**.

---

## 4. Список пациентов (Scroll View)

1. Правый клик по **Panel** → **UI** → **Scroll View**.
2. Переименуй в **PatientsScrollView**.
3. **Rect Transform** (например под полем поиска):
   - Anchor: stretch по горизонтали, сверху (Top Stretch).
   - Left: 20, Right: 20, Top: 80, Height: 220 (или как удобно).
4. Внутри Scroll View есть:
   - **Viewport** (маска, за которую не выходит контент).
   - Внутри Viewport — **Content**.

5. Выбери **Content** (именно он нужен для скрипта):
   - **Rect Transform**: Anchor Top (Top Stretch), Pivot (0.5, 1).
   - Left/Right: 0, Top: 0, Height: 200 (высота меняется скриптом при заполнении).
   - **Content Size Fitter** (Add Component): Vertical Fit = Preferred Size — опционально, скрипт сам выставляет высоту.

Запомни: скрипту нужен именно этот **Content** (дочерний объект Viewport) — его перетащишь в поле **Patients Content**.

---

## 5. Блок для сессий

1. Правый клик по **Panel** → **UI** → **Scroll View**.
2. Переименуй в **SessionsScrollView**.
3. **Rect Transform** (под списком пациентов):
   - Anchor: Top Stretch, Left: 20, Right: 20, Top: 310, Bottom: 80 (или одна Height, например 250).
4. Внутри снова **Viewport** → **Content**.
5. Выбери этот **Content**: Anchor Top, Pivot (0.5, 1), Height любая (скрипт подставит).

Этот **Content** перетащишь в поле **Sessions Content** в скрипте.

Опционально над этим Scroll View можно добавить **Text** с подписью "Sessions du patient" / "Сессии пациента".

---

## 6. Привязать скрипт и ссылки

1. Создай пустой объект: правый клик в Hierarchy → **Create Empty**, назови например **HospitalUI**.
2. **Add Component** → найди **Hospital Patients List UI** (скрипт из папки HospitalData/UI).
3. В **Inspector** у скрипта будут поля:
   - **Search Field** — перетащи сюда объект **SearchField** (твой Input Field).
   - **Patients Content** — перетащи **Content** из первого Scroll View (список пациентов).
   - **Sessions Content** — перетащи **Content** из второго Scroll View (сессии).
   - **Patient Row Height** (опционально): высота одной строки пациента, по умолчанию 40.

Сохрани сцену и запусти Play: список пациентов подтянется из данных, ввод в поле поиска будет фильтровать по первым буквам имени/фамилии, при клике на пациента справа (или ниже) появятся его сессии.

---

## Как редактировать под себя

- **Тексты**  
  В коде можно поменять:
  - Подпись кнопки пациента: в `CreatePatientButton` строка с `text.text = $"{patient.Prenom} {patient.Nom} — {patient.Pathologie}"` — добавь/убери поля (например дата рождения, пол).
  - Строку сессии: в `CreateSessionRow` строка `text.text = $"{session.DateDebut:g} — ..."` — поменяй формат даты или набор полей (duree, ScoreTotal, Commentaire и т.д.).

- **Цвета**  
  В коде:
  - `img.color = new Color(0.25f, 0.25f, 0.35f, 0.95f)` — цвет фона кнопки пациента.
  - `img.color = new Color(0.2f, 0.3f, 0.25f, 0.9f)` — цвет строки сессии.
  Меняй R, G, B (0–1) и последнее число — прозрачность.

- **Размеры**  
  - Высота строки пациента: в Inspector поле **Patient Row Height** или в коде константа `40f`.
  - Высота строки сессии: в коде `SessionRowHeight = 32f` — поменяй при необходимости.

- **Поиск**  
  Логика в `MatchPrefix`: сейчас совпадение по началу имени, фамилии и по "Prenom Nom" / "Nom Prenom". Чтобы искать по подстроке (не только с начала), можно заменить `StartsWith` на `Contains`.

После правок сохрани скрипт; сцену перезапускать не обязательно — при следующем Play изменения уже будут учтены.
