



using ExcelDataReader;
using System.Text;

namespace Quiz_3
{
    internal class Program
    {


        private static List<Question> questions = new List<Question>();
        private static Random random = new Random();

        static void Main(string[] args)
        {
            string filePath = @"Quiz3.xlsx";
            questions = ReadExcelWithDataReader(filePath);

            if (questions.Count == 0)
            {
                Console.WriteLine("Не удалось загрузить вопросы!");
                return;
            }

            Console.WriteLine($"Загружено вопросов: {questions.Count}");
            Console.WriteLine("Нажмите ENTER для начала...");
            Console.ReadLine();

            RunQuiz();
        }

        static List<Question> ReadExcelWithDataReader(string filePath)
        {
            var questions = new List<Question>();

            try
            {
                // Важно для кодировки
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        bool isFirstRow = true;

                        while (reader.Read())
                        {
                            if (isFirstRow)
                            {
                                isFirstRow = false;
                                continue; // Пропускаем заголовок
                            }
                           
                            if (reader.FieldCount >= 3)
                            {
                                if (reader.GetValue(1) == null || reader.GetValue(1).ToString() == "") break;
                                questions.Add(new Question
                                {
                                    Id = Convert.ToInt32(reader.GetValue(0)),
                                    QuestionText = reader.GetValue(1)?.ToString() ?? "",
                                    Answer = reader.GetValue(2)?.ToString() ?? "",
                                    Priority = reader.FieldCount > 3 ? Convert.ToInt32(reader.GetValue(3)) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            }

            return questions;
        }

        // Метод для рандомного выбора вопроса с учетом приоритета
        static Question GetRandomQuestionWithPriority()
        {
            if (questions.Count == 0) return null;

            var weightedQuestions = new List<Question>();

            // Создаем взвешенный список на основе приоритетов
            foreach (var question in questions)
            {
                int weight = question.Priority switch
                {
                    0 => 2,  // 20% шанс (2 из 10)
                    1 => 3,  // 30% шанс (3 из 10)
                    2 => 5,  // 50% шанс (5 из 10)
                    _ => 2   // по умолчанию
                };

                // Добавляем вопрос несколько раз согласно весу
                for (int i = 0; i < weight; i++)
                {
                    weightedQuestions.Add(question);
                }
            }

            // Выбираем случайный вопрос из взвешенного списка
            return weightedQuestions[random.Next(weightedQuestions.Count)];
        }

        // Основной метод викторины
        static void RunQuiz()
        {
            Console.Clear();
            Console.WriteLine("=== ВИКТОРИНА ===");
            Console.WriteLine("ПРОБЕЛ - показать ответ");
            Console.WriteLine("ENTER - следующий вопрос");
            Console.WriteLine("ESC - выход\n");

            while (true)
            {
                // Получаем случайный вопрос
                var question = GetRandomQuestionWithPriority();

                if (question == null)
                {
                    Console.WriteLine("Нет доступных вопросов!");
                    break;
                }

                // Выводим вопрос с ID и приоритетом
                DisplayQuestion(question);

                // Ждем нажатия клавиши
                var key = Console.ReadKey(true).Key;

                // Обрабатываем клавиши
                while (key != ConsoleKey.Enter && key != ConsoleKey.Escape)
                {
                    if (key == ConsoleKey.Spacebar)
                    {
                        DisplayAnswer(question);
                    }

                    key = Console.ReadKey(true).Key;
                }
                Console.Clear();

                if (key == ConsoleKey.Escape)
                    break;

                Console.WriteLine("\n" + new string('-', 50));
            }

            Console.WriteLine("\nВикторина завершена! Нажмите любую клавишу...");
            Console.ReadKey();
        }

        // Метод для отображения вопроса
        static void DisplayQuestion(Question question)
        {
            string priorityText = question.Priority switch
            {
                0 => "Низкий",
                1 => "Средний",
                2 => "Высокий",
                _ => "Неизвестно"
            };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[ID: {question.Id} | Приоритет: {priorityText}]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"ВОПРОС: {question.QuestionText}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("ПРОБЕЛ - показать ответ | ENTER - следующий | ESC - выход");
            Console.ResetColor();
        }

        // Метод для отображения ответа
        static void DisplayAnswer(Question question)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nОТВЕТ: {question.Answer}");
            Console.ResetColor();
        }
    }

    class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = "";
        public string Answer { get; set; } = "";
        public int Priority { get; set; }
    }
}

