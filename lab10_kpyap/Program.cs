using System;
using System.Collections;

interface IRateAndCopy
{
    double Rating { get; }
    object DeepCopy();
}

class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }

    public Person(string firstName, string lastName, DateTime birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }

    public Person() : this("Неизвестно", "Неизвестно", DateTime.Now) { }

    public override bool Equals(object obj)
    {
        if (obj is Person other)
        {
            return FirstName == other.FirstName && LastName == other.LastName && BirthDate == other.BirthDate;
        }
        return false;
    }

    public static bool operator ==(Person p1, Person p2) => p1?.Equals(p2) ?? p2 is null;
    public static bool operator !=(Person p1, Person p2) => !(p1 == p2);

    public override int GetHashCode() => HashCode.Combine(FirstName, LastName, BirthDate);

    public virtual object DeepCopy() => new Person(FirstName, LastName, BirthDate);

    public override string ToString() => $"{FirstName} {LastName}, дата рождения: {BirthDate.ToShortDateString()}";
}

class Article : IRateAndCopy
{
    public Person Author { get; set; }
    public string Title { get; set; }
    public double Rating { get; set; }

    public Article(Person author, string title, double rating)
    {
        Author = author;
        Title = title;
        Rating = rating;
    }

    public Article() : this(new Person(), "Неизвестно", 0.0) { }

    public object DeepCopy() => new Article((Person)Author.DeepCopy(), Title, Rating);

    public override string ToString() => $"{Title} от {Author}, Рейтинг: {Rating}";
}

class Edition
{
    protected string title;
    protected DateTime releaseDate;
    protected int circulation;

    public string Title
    {
        get { return title; }
        set { title = value; }
    }

    public DateTime ReleaseDate
    {
        get { return releaseDate; }
        set { releaseDate = value; }
    }

    public int Circulation
    {
        get { return circulation; }
        set
        {
            if (value < 0)
                throw new ArgumentException("Тираж не может быть отрицательным");
            circulation = value;
        }
    }

    public Edition(string title, DateTime releaseDate, int circulation)
    {
        Title = title;
        ReleaseDate = releaseDate;
        Circulation = circulation;
    }

    public Edition() : this("Неизвестно", DateTime.Now, 0) { }

    public virtual object DeepCopy() => new Edition(Title, ReleaseDate, Circulation);

    public override bool Equals(object obj)
    {
        if (obj is Edition other)
        {
            return Title == other.Title && ReleaseDate == other.ReleaseDate && Circulation == other.Circulation;
        }
        return false;
    }

    public static bool operator ==(Edition e1, Edition e2) => e1?.Equals(e2) ?? e2 is null;
    public static bool operator !=(Edition e1, Edition e2) => !(e1 == e2);

    public override int GetHashCode() => HashCode.Combine(Title, ReleaseDate, Circulation);

    public override string ToString() => $"{Title}, Дата выхода: {ReleaseDate.ToShortDateString()}, Тираж: {Circulation}";
}

class Magazine : Edition, IRateAndCopy, IEnumerable
{
    private Frequency frequency;
    private ArrayList editors = new ArrayList();
    private ArrayList articles = new ArrayList();

    public double Rating => articles.Count == 0 ? 0 : ((Article[])articles.ToArray(typeof(Article))).Average(a => a.Rating);

    public Magazine(string title, Frequency frequency, DateTime releaseDate, int circulation)
        : base(title, releaseDate, circulation)
    {
        this.frequency = frequency;
    }

    public Magazine() : this("Неизвестно", Frequency.Monthly, DateTime.Now, 0) { }

    public void AddEditors(params Person[] editors)
    {
        this.editors.AddRange(editors);
    }

    public void AddArticles(params Article[] articles)
    {
        this.articles.AddRange(articles);
    }

    public override object DeepCopy()
    {
        var copy = new Magazine(Title, frequency, ReleaseDate, Circulation);
        foreach (Person editor in editors)
        {
            copy.editors.Add((Person)editor.DeepCopy());
        }
        foreach (Article article in articles)
        {
            copy.articles.Add((Article)article.DeepCopy());
        }
        return copy;
    }

    public override string ToString()
    {
        string editorList = string.Join(", ", editors.ToArray());
        string articleList = string.Join("\n", articles.ToArray());
        return $"{base.ToString()}, Частота: {frequency}\nРедакторы: {editorList}\nСтатьи:\n{articleList}";
    }

    public string ToShortString() => $"{base.ToString()}, Частота: {frequency}, Средний рейтинг: {Rating}";

    public IEnumerable GetArticlesByRating(double minRating)
    {
        foreach (Article article in articles)
        {
            if (article.Rating > minRating)
            {
                yield return article;
            }
        }
    }

    public IEnumerable GetArticlesByTitle(string keyword)
    {
        foreach (Article article in articles)
        {
            if (article.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                yield return article;
            }
        }
    }

    public IEnumerable GetEditorsWithoutArticles()
    {
        foreach (Person editor in editors)
        {
            bool hasArticle = false;
            foreach (Article article in articles)
            {
                if (article.Author == editor)
                {
                    hasArticle = true;
                    break;
                }
            }
            if (!hasArticle)
            {
                yield return editor;
            }
        }
    }

    public IEnumerator GetEnumerator()
    {
        foreach (Article article in articles)
        {
            if (!editors.Contains(article.Author))
            {
                yield return article;
            }
        }
    }
}

enum Frequency { Weekly, Monthly, Yearly }

class Program
{
    static void Main()
    {
        try
        {
            var edition1 = new Edition("Наука", DateTime.Now, 1000);
            var edition2 = new Edition("Наука", DateTime.Now, 1000);

            Console.WriteLine($"Издание1 == Издание2: {edition1 == edition2}");
            Console.WriteLine($"Хеш1: {edition1.GetHashCode()}, Хеш2: {edition2.GetHashCode()}");

            edition1.Circulation = -500; // Вызовет исключение
        }
        catch (ArgumentException e)
        {
            Console.WriteLine($"Исключение: {e.Message}");
        }

        var editor1 = new Person("Иван", "Иванов", new DateTime(1980, 5, 15));
        var editor2 = new Person("Мария", "Петрова", new DateTime(1990, 7, 20));

        var article1 = new Article(editor1, "Квантовая физика", 4.5);
        var article2 = new Article(editor2, "Искусственный интеллект", 3.8);

        var magazine = new Magazine("Технологии", Frequency.Monthly, DateTime.Now, 2000);
        magazine.AddEditors(editor1, editor2);
        magazine.AddArticles(article1, article2);

        Console.WriteLine(magazine);

        var copy = (Magazine)magazine.DeepCopy();
        magazine.Title = "Будущее технологий";
        Console.WriteLine("\nИсходный журнал:\n" + magazine);
        Console.WriteLine("\nКопия журнала:\n" + copy);

        Console.WriteLine("\nСтатьи с рейтингом > 4.0:");
        foreach (Article article in magazine.GetArticlesByRating(4.0))
        {
            Console.WriteLine(article);
        }

        Console.WriteLine("\nСтатьи с ключевым словом 'интеллект':");
        foreach (Article article in magazine.GetArticlesByTitle("интеллект"))
        {
            Console.WriteLine(article);
        }

        Console.WriteLine("\nРедакторы без статей:");
        foreach (Person editor in magazine.GetEditorsWithoutArticles())
        {
            Console.WriteLine(editor);
        }
    }
}
