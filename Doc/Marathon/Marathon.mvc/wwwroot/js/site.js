
// ===============================
// CATEGORY POPUP
// ===============================

function openCategoryModal() {
    const modal = document.getElementById("categoryModal");
    if (modal) modal.style.display = "flex";
}

function closeCategoryModal() {
    const modal = document.getElementById("categoryModal");
    if (modal) modal.style.display = "none";
}


// ===============================
// EVENT POPUP
// ===============================

function openEventModal() {
    const modal = document.getElementById("eventModal");
    if (modal) modal.style.display = "flex";
}

function closeEventModal() {
    const modal = document.getElementById("eventModal");
    if (modal) modal.style.display = "none";
}


// ===============================
// CLOSE POPUPS
// ===============================

document.addEventListener("click", function (event) {

    if (event.target.id === "categoryModal") {
        closeCategoryModal();
    }

    if (event.target.id === "eventModal") {
        closeEventModal();
    }
});

document.addEventListener("keydown", function (event) {

    if (event.key === "Escape") {
        closeCategoryModal();
        closeEventModal();
    }
});


// ===============================
// CREATE CATEGORY
// ===============================

document.addEventListener("DOMContentLoaded", function () {

    const categoryForm = document.getElementById("categoryForm");

    if (!categoryForm) return;

    categoryForm.addEventListener("submit", async function (event) {

        event.preventDefault();

        const message = document.getElementById("categoryMessage");
        const saveButton = categoryForm.querySelector(".save-btn");

        const categoryData = {
            eventId: document.getElementById("EventId").value,
            categoryName: document.getElementById("CategoryName").value.trim(),
            minAge: Number(document.getElementById("MinAge").value),
            maxAge: Number(document.getElementById("MaxAge").value),
            genderRestrictions: document.getElementById("GenderRestrictions").value,
            maxParticipants: Number(document.getElementById("MaxParticipants").value),
            createdBy: "system",
            createdAt: new Date().toISOString()
        };

        // Validate form
        if (!categoryData.eventId) {
            message.textContent = "Please select an event.";
            message.style.color = "red";
            return;
        }

        if (!categoryData.categoryName) {
            message.textContent = "Please enter a category name.";
            message.style.color = "red";
            return;
        }

        if (categoryData.minAge > categoryData.maxAge) {
            message.textContent = "Minimum age cannot exceed maximum age.";
            message.style.color = "red";
            return;
        }

        if (categoryData.maxParticipants <= 0) {
            message.textContent = "Maximum participants must be greater than zero.";
            message.style.color = "red";
            return;
        }

        // Get antiforgery token
        const tokenInput = categoryForm.querySelector(
            'input[name="__RequestVerificationToken"]'
        );

        if (!tokenInput) {
            message.textContent = "Security token not found. Refresh the page.";
            message.style.color = "red";
            return;
        }

        saveButton.disabled = true;
        saveButton.textContent = "Saving...";

        message.textContent = "";

        try {

            // Send data to MVC CategoryController
            const response = await fetch("/Category/Create", {

                method: "POST",

                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": tokenInput.value
                },

                body: JSON.stringify(categoryData)
            });

            const result = await response.text();

            if (!response.ok) {
                throw new Error(result || "Could not create category.");
            }

            message.textContent = "Category created successfully!";
            message.style.color = "green";

            categoryForm.reset();

        }
        catch (error) {

            console.error("Category creation error:", error);

            message.textContent = error.message;
            message.style.color = "red";

        }
        finally {

            saveButton.disabled = false;
            saveButton.textContent = "Save Category";

        }

    });

});
// ===============================
// SUBMIT EVENT
// ===============================

document.addEventListener("DOMContentLoaded", function () {

    const eventForm = document.getElementById("eventForm");

    if (!eventForm) return;

    eventForm.addEventListener("submit", async function (event) {

        event.preventDefault();

        const message = document.getElementById("eventMessage");
        const saveButton = eventForm.querySelector(".save-btn");

        const eventData = {
            eventName: document.getElementById("EventName").value.trim(),
            eventDate: document.getElementById("EventDate").value,
            location: document.getElementById("Location").value.trim(),
            venue: document.getElementById("Venue").value.trim(),
            distanceKm: Number(document.getElementById("DistanceKm").value),
            eventFee: Number(document.getElementById("EventFee").value),
            eventType: document.getElementById("EventType").value,
            status: document.getElementById("Status").value
        };

        saveButton.disabled = true;
        saveButton.textContent = "Saving...";

        try {

            const tokenInput = eventForm.querySelector(
                'input[name="__RequestVerificationToken"]'
            );

            if (!tokenInput) {
                throw new Error("Security token missing. Refresh the page.");
            }

            const response = await fetch("/Events/Create", {

                method: "POST",

                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": tokenInput.value
                },

                body: JSON.stringify(eventData)
            });

            const result = await response.text();

            if (!response.ok) {
                throw new Error(result || "Failed to create event.");
            }

            message.textContent = "Event created successfully!";
            message.style.color = "green";

            eventForm.reset();

        }
        catch (error) {

            console.error("Event creation error:", error);

            message.textContent = error.message;
            message.style.color = "red";

        }
        finally {

            saveButton.disabled = false;
            saveButton.textContent = "Submit Event";

        }

    });

});