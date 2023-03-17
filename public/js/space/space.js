import API from "../API/API.js";

const backSidebar = document.querySelector(".panel-insert-car");
const Form = document.querySelector(".panel-insert-car form");
const openWindowCreateUser = document.querySelector(
  ".header-spaces > button.add-space"
);
const closeWindowCreateUser = document.querySelector(
  ".panel-insert-car .head-panel > button"
);
const insertNewUserInSpace = document.querySelector(
  ".panel-insert-car form .more-field .field button"
);

openWindowCreateUser.onclick = () => {
  showForm();
};

closeWindowCreateUser.onclick = (e) => {
  if (e.target.tagName === "BUTTON" || e.target.tagName === "I") {
    backSidebar.classList.remove("active");
  }
};

function showForm() {
  backSidebar.classList.add("active");
}
insertNewUserInSpace.onclick = async () => {
  const formData = new FormData(Form);
  const response = await API.spaceService.newSpace();

  console.log(response);
};

/* var allSpace = [];
const alphabet = "ABCDEFGHIJKLMNOQRSTUVWXYZ".split("");
for (var i = 0; i < alphabet.length; i++){
    var t = 1;
    for(; t <= 10; t++){
        allSpace.push(alphabet[i]+t)
    }
} */
