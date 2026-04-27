# Audio Assets List

Audio files are split into two categories: **Sound Effects** (self-contained clips played as single files) and **Spoken Audio** (voice clips that may be chained together in sequence to form composite phrases). See the bracketed audio system description in WordChompers.md for how composite phrases work.

All spoken audio should be recorded in a consistent voice and tone so that chained clips sound natural when played in sequence.

---

## Sound Effects

These are played as single, complete sounds for a specific game event.

| File Name | Trigger | Notes |
|---|---|---|
| `sfx_player_move` | Player moves one cell in any direction | Short, light movement sound |
| `sfx_player_chomp_valid` | Player eats a word/letter that matches the category/target | Munching/eating sound |
| `sfx_level_complete` | Player eats the last valid word/letter in the grid | Fanfare |
| `sfx_game_over` | Player loses their last life | Game over sting |
| `sfx_monster_eat` | A monster eats the player, or one monster eats another | Single eating sound used for both cases |
| `sfx_player_scream` | A monster eats the player | Silly scream; plays alongside `sfx_monster_eat` |
| `sfx_menu_button_press` | Player presses "1. Chomp Letters" or "2. Chomp Words" on the opening screen | UI confirmation sound |

---

## Spoken Audio — Fixed Phrase Fragments

These are the non-variable parts of composite spoken phrases. They are played in sequence with variable clips to form full sentences.

| File Name | Spoken Content | Used In |
|---|---|---|
| `spoken_is_not_a` | "is not a" | Chomp Words invalid chomp: `[word]` + `spoken_is_not_a` + `[category]` |
| `spoken_chomp_the_letter` | "Chomp the letter" | Chomp Letters announcement: `spoken_chomp_the_letter` + `[letter]` |
| `spoken_you_chomped` | "You chomped" | Chomp Letters invalid chomp: `spoken_you_chomped` + `[chomped letter]` + `spoken_only_chomp` + `[target letter]` |
| `spoken_only_chomp` | "Only chomp" | Chomp Letters invalid chomp (see above) |

---

## Spoken Audio — Category Headers

One file per category. Played when a Chomp Words level launches and on the repeating reminder timer. Each file speaks the full Level Header string for that category.

These are whole-phrase recordings, not composites.

| File Name | Spoken Content |
|---|---|
| `header_food` | "Eat All Words That Are Food" |
| `header_animals` | "Eat All Words That Are Animals" |
| `header_reptiles` | "Eat All Words That Are Reptiles" |
| `header_mammals` | "Eat All Words That Are Mammals" |

> **Note:** One header file must be added for every category added to the category data file in the future.

---

## Spoken Audio — Category Names

One file per category. Used as the variable `[category]` fragment in the composite phrase `[word] is not a [category]`.

| File Name | Spoken Content |
|---|---|
| `category_food` | "Food" |
| `category_animals` | "Animals" |
| `category_reptiles` | "Reptiles" |
| `category_mammals` | "Mammals" |

> **Note:** One category name file must be added for every category added to the category data file in the future.

---

## Spoken Audio — Individual Words

One file per word in the word data file. Used as the variable `[chomped word]` fragment in the composite phrase `[word] is not a [category]`.

**File naming convention:** `word_[word].wav` — for example, `word_chicken.wav`, `word_apple.wav`.

The exact list depends on the contents of the word data file. Every word that appears in the word file must have a corresponding spoken audio clip.

> **Note:** One word file must be added for every word added to the word data file in the future.

---

## Spoken Audio — Letters A–Z

One file per letter. Used as variable fragments in:
- Chomp Letters level announcement: `spoken_chomp_the_letter` + `[letter]`
- Chomp Letters invalid chomp: `spoken_you_chomped` + `[chomped letter]` + `spoken_only_chomp` + `[target letter]`

| File Name | Spoken Content |
|---|---|
| `letter_a` | "A" |
| `letter_b` | "B" |
| `letter_c` | "C" |
| `letter_d` | "D" |
| `letter_e` | "E" |
| `letter_f` | "F" |
| `letter_g` | "G" |
| `letter_h` | "H" |
| `letter_i` | "I" |
| `letter_j` | "J" |
| `letter_k` | "K" |
| `letter_l` | "L" |
| `letter_m` | "M" |
| `letter_n` | "N" |
| `letter_o` | "O" |
| `letter_p` | "P" |
| `letter_q` | "Q" |
| `letter_r` | "R" |
| `letter_s` | "S" |
| `letter_t` | "T" |
| `letter_u` | "U" |
| `letter_v` | "V" |
| `letter_w` | "W" |
| `letter_x` | "X" |
| `letter_y` | "Y" |
| `letter_z` | "Z" |

---

## Summary of File Counts

| Category | Count |
|---|---|
| Sound effects | 7 |
| Fixed spoken fragments | 4 |
| Category header phrases | 4 (+ 1 per future category) |
| Category name fragments | 4 (+ 1 per future category) |
| Individual word clips | 1 per word in the word file |
| Letter clips (A–Z) | 26 |
| **Minimum total (excluding word clips)** | **45** |
