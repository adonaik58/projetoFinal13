import API from "../API/API.js";

const tabs = document.querySelectorAll(".tab-content .tab-appear .tab");
const buttons = document.querySelectorAll(".tab-content .header-tab .head-tab button");
const tabLine = document.querySelector(".content-all .tab-content .header-tab .head-tab .tabLine");

tabs[0].style.display = "block";

buttons.forEach(function (button, i) {
  button.onclick = function (r) {
    for (var j = 0; j < tabs.length; j++) {
      tabs[j].style.display = "none";
      buttons[j].classList.remove("active");
    }
    tabs[i].style.display = "block";
    button.classList.add("active");

    if (i == 1) {
      tabLine.classList.add("isOne");
    } else {
      tabLine.classList.remove("isOne");
    }
  };
});

const form = document.querySelector("form");

const save = document.querySelector(".save");

save.onclick = async () => {
  const formData = new FormData(form);
  const response = await API.settingsService.update(formData);
  FNtoast(response);

  console.log(response);
};

function FNtoast(response) {
  const toast = document.querySelector(".drt .toast");
  const icon = toast.querySelector(".icon i");

  toast.classList.add(response.status ? "success" : "error");
  icon.classList.add(response.status ? "fa-check" : "fa-times");

  toast.querySelector(".text p").textContent = response.message;
  setTimeout(() => {
    icon.classList.remove(response.status ? "fa-check" : "fa-times");
    toast.classList.remove(response.status ? "success" : "error");
    console.log(toast.classList);
  }, 6000);
}

getAutoComplete();
async function getAutoComplete() {
  const input1 = document.querySelector("form .more-field .field:nth-child(1) > input");
  const input2 = document.querySelector("form .more-field .field:nth-child(2) > input");
  const input3 = document.querySelector("form .more-field .field:nth-child(3) > input");

  const autoComplete = await API.settingsService.autoComplete();
  console.log(autoComplete);
  console.log(input1);
  console.log(input2);
  console.log(input3);
  input1.value = autoComplete?.data.renda;
  input2.value = autoComplete?.data.num;
  input3.value = autoComplete?.data.quant;
}
