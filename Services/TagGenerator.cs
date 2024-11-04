using System;
using System.Collections.Generic;
using System.Linq;

public class SimpleTagExtractor
{
    // High-priority ingredients
    private readonly List<string> _priorityIngredients = new List<string>
    {
        "chicken", "beef", "tofu", "pork", "shrimp", "fish", "egg"
    };

    // Regular ingredients
    private readonly List<string> _ingredients = new List<string>
    {
        "sauce", "noodle", "rice", "bean", "vegetable", "mushroom", "tomato", "potato",
        "carrot", "cabbage", "spinach", "broccoli", "onion", "garlic", "ginger", "lemon",
        "lime", "cilantro", "basil", "parsley", "mint", "chili", "chickpea", "lentil",
        "bacon", "sausage", "lobster", "crab", "clams", "oyster", "salmon", "tuna",
        "avocado", "corn", "peas", "zucchini", "bell pepper", "green bean", "cauliflower",
        "eggplant", "pumpkin", "beet", "okra", "scallop", "duck", "lamb", "veal",
        "turkey", "meatball"
    };

    private readonly List<string> _textures = new List<string>
    {
        "chewy", "soft", "crispy", "crunchy", "tender", "juicy", "silky", "creamy",
        "smooth", "velvety", "gooey", "fluffy", "firm", "brittle", "sticky", "dense",
        "light", "airy", "moist", "mushy", "gelatinous", "gritty", "powdery", "greasy",
        "oily", "crumbly", "rubbery", "spongy", "stringy", "crispy", "crackly", "crisp",
        "charred", "toasty", "melty", "flaky", "bubbly", "caramelized", "fizzy"
    };

    private readonly List<string> _flavors = new List<string>
    {
        "spicy", "sweet", "savory", "sour", "bitter", "tangy", "umami", "earthy",
        "herbal", "fruity", "nutty", "smoky", "zesty", "pungent", "peppery",
        "floral", "garlicky", "buttery", "salty", "chocolaty", "citrusy",
        "acidic", "vanilla", "caramel", "minty", "refreshing", "gingery",
        "creamy", "malty", "barbecue", "honeyed", "mellow", "sharp", "robust",
        "roasted", "tart", "warming", "meaty"
    };

    public List<string> ExtractTags(string description)
    {
        var tags = new List<string>();

        // First, try to find a high-priority ingredient
        var ingredientTag = _priorityIngredients
            .FirstOrDefault(ingredient => description.Contains(ingredient, StringComparison.OrdinalIgnoreCase));

        // If no priority ingredient is found, search in the regular ingredients list
        if (ingredientTag == null)
        {
            ingredientTag = _ingredients
                .FirstOrDefault(ingredient => description.Contains(ingredient, StringComparison.OrdinalIgnoreCase));
        }

        // Add the ingredient tag if found
        if (ingredientTag != null) tags.Add(ingredientTag);

        // Find one matching texture
        var textureTag = _textures
            .FirstOrDefault(texture => description.Contains(texture, StringComparison.OrdinalIgnoreCase));
        if (textureTag != null) tags.Add(textureTag);

        // Find one matching flavor
        var flavorTag = _flavors
            .FirstOrDefault(flavor => description.Contains(flavor, StringComparison.OrdinalIgnoreCase));
        if (flavorTag != null) tags.Add(flavorTag);

        // Limit to a maximum of 3 tags (1 from each category if available)
        return tags.Take(3).ToList();
    }
}
