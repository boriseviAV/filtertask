# Графический фильтр
### Лабораторная работа #3
[[презентация]](https://www.dropbox.com/s/cpjm9szra28zm1y/Task%203.pptx?dl=0) [[репозиторий]](https://github.com/Andrew414/filtertask) [[english]](https://github.com/Andrew414/filtertask/blob/master/README.md)

### Условие
Необходимо реализовать программу, которая может применять различные фильтры к изображениям и измерять их производительность. Фильтры должны быть распараллелены с помощью парадигмы MAP, то есть каждый пиксель выходного изображения должен быть вычислен независимо от других пикселей, на основе одного или нескольких пикселей из входного изображения. 

Вы должны измерять производительность фильтров и выводить результаты на консоль (либо отображать в графическом интерфейсе). Необходимо показывать, сколько мегапикселей и мегабайт могут быть обработаны в секунду.

Вам необходимо выбрать один или несколько фильтров и записать их в файле `README.md` в своем персональном каталоге в репозитории.

Примеры фильтров:
* Фильтры, изменяющие цветовые каналы
  * Фильтры, добавляющие или удаляющие цветовые каналы в изображении
  * Фильтры, преобразовывающие цветные изображения в черно-белые
  * Фильтры, бинаризующие изображения на основе разных порогов
  * Фильтры, изменяющие яркость пикселей на основе биективных функций преобразования яркости
* Линейные фильтры
  * Низкочастотные фильтры (размытие изображений)
  * Высокочастотные фильтры (увеличение резкости изображений)
* Нелинейные фильтры
  * Медианный фильтр
  * Морфологический наращивающий фильтр
  * Морфологический эрозионный фильтр
* Обработка гистограммы изображениям
  * Выравнивание гистограммы
  * Построение гистограммы (формально, это не графический фильтр, но его также можно реализовать)
 

### Дополнительные задания
Три задачи:
* Реализовать 3 и более фильтров
  * Фильтры должны быть из разных категорий, например, нельзя выбрать два разных низкочастотных и один высокочастотный фильтр, так как их реализации будут почти одинаковыми. Фильтры должны быть выбраны через интерфейс командной строки, через графический интерфейс, либо переданы как параметры
* Реализовать фильтры как ядра GPGPU
  * Реализация фильтров должна быть написана на `CUDA` либо на похожем языке (ядра `OpenCL`, шейдеры и др.), компилируемом в код для видеокарт. Кроме того, в **явном** виде должны быть блоки кода для ЦП и для видеокарты, управление памятью видеокарты и запуск фильтров на ней (либо на симуляции). Использование высокоуровневых оберток над библиотеками GPGPU не засчитывается 
* Добавить графический интерфейс
  * Интерфейс должен позволять выбирать входные и выходные изображения, фильтры для применения, кроме того, он должен показывать прогресс и статистику применения фильтров

### Репозиторий
[Репозиторий](https://github.com/Andrew414/filtertask) содержит главную страницу [`README.md`](https://github.com/Andrew414/filtertask/blob/master/README.rus.md) и четырнадцать персональных каталогов. Необходимо сделать копию (fork) репозитория и вносить все изменения только в свой персональный каталог. Все эти каталоги содержат пустой файл `.gitignore`, куда нужно вписать все временные файлы из лабораторной работы. Также, в этой лабораторной работе нужно создать файл `README.md` и записать туда выбранные фильтры. Остальные файлы должны быть файлами кода (в т.ч. проекта).

### Сдача лабораторной работы
Можно выбрать любой язык программирования, IDE и фреймворки/библиотеки. Если для работы нужны какие-то нестандартные конфигурации или ПО, предоставьте инструкцию по настройке среды.

Запрещено использовать библиотеки и фреймворки, которые выполняют работу, близкую к условию лабораторной. Например, можно использовать средства для парсинга форматов изображений, для того, чтобы можно было в удобной форме работать с изображением попиксельно, но нельзя использовать OpenCV для многопоточной обработки изображения целиком. 

Сдача через **Pull Request** из вашего клонированного репозитория. Pull Request должен содержать **только код, файлы проекта (включая, возможно, тестовые файлы) и информационные файлы (.gitignore, README.md)**. Результаты сборок, временные файлы и скачанный архив сайта должны быть исключены из коммитов.

### Оценка
Максимум за лабораторную работу - **10 баллов**.
- **4 балла** за написанный фильтр и тесты производительности (главная задача)
- **3 балла**, если работа сделана вовремя
- **1 балл** за реализацию *трех и более* разных фильтров
- **1 балл** за реализацию фильтров как *ядер GPGPU*
- **1 балл** за реализацию *графического интерфейса*

### Сроки
На выполнение лабораторной дается 4 недели (3 апреля - 1 мая). Лабораторная работа считается принятой, когда Pull Request переходит в состояние "approved". Дни, в течение которых лабораторная была на проверке, не учитываются.
![ ](https://i.snag.gy/lPOzf7.jpg)

Например, если вы сдаете лабораторную через три недели после выдачи (24 апреля), но я даю комментарии только через неделю после дедлайна (8 мая), у вас все еще будет неделя на устранение недочетов, все дни после коммита до получения ревью не считаются.


### Желаю удачи!
