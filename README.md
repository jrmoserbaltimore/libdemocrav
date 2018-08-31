# libdemocrav
Core library for Electronic Voting Machine (EVM) software.

The **exploratory** branch is used to track pilot code and explore architecture and implementation strategies; it is often a mess and in flux.

# Elections Integrity
Electronic Voting Machines allow the implementation of voting rules which resist elections attacks by strategic nomination, candidate withdrawal, and organized tactical bloc voting.  Advanced election strategies to expand voter choice, provide proportional nomination and representation, and to elect a consensus candidate via Smith- and Schwartz-efficient voting rules can resist political manipulation carried out by well-timed media propaganda.

Voting rules such as Tideman's Alternative Smith specifically resist manipulation, while Single Transferable Vote inherently resists manipulation due to its proportional nature.  By using Electoral Fusion and nominating up to two candidates per party via Single Transferable Vote, an elections authority can increase the span of voter choice while reducing the size of the candidate pool to avoid voter fatigue.

This contrasts with today's Primary elections, which generally draw participation from the more-active members of each party, among which the voters lean further to the extremes and trend toward more-extreme candidates.  Besides excluding many voters from this process and skewing the results away from the less-invested party-affiliated voters, today's primary elections allow a well-timed media campaign to shift nomination to or away from a more-extreme candidate.  More-moderate voters and swing voters then find themselves without viable candidates, even if given a Condorcet voting method.

These voting rules, while relatively simple—Tideman's Alternative Smith simply eliminates all candidates except the smallest set who would defeat all others one-on-one, then eliminates whichever has the least votes if there is more than one and repeats the whole process—require a tedious process of counting ranked ballots.  Human counting of current paper ballots is open to manipulation by colluding election judges; ranked ballot counting allows for more sleight-of-hand manipulation of the error rate.  Further, our paper ballot system doesn't provide any integrity guarantees during recounts, as the votes have left the view of the public observers and cannot be proven untampered.

Current security problems surrounding EVMs has made it abundantly clear nobody has considered election integrity.  In response, states which have switched to paper ballots often fail to adopt well-developed paper ballot voting security standards which, while limited, do provide high degrees of inegrity.  Many states lack an electronic record of votes, carrying out all counts by hand, and on extremely rare occasion "finding" a ballot box during recount—recounts which occur after votes have left public view, when colluding officials could make additional marks on ballots where all votes have not been exhausted.

We cannot trust elections; we must observe, and we must prove what is counted is what we observe.

# Core Aspects of EVM Software

Electronic Voting Machines must achieve a number of requirements:

* **Integrity**.  An EVM maker must design around an elections integrity model allowing voters and the general public to validate election results independently.  Among other things, EVMs must be verifiably-untampered at poll open, untamperable during the election, and able to demonstrate the vote count prior to exposing ballots to tampering so public observers can later verify that the votes counted by the elections authority are the same votes observed at the polling locations.
* **Cost**.  The State of Maryland paid $65 million for voting machines in 2002, and replaced them with $28 million of paper ballot scanners in 2016.  Maryland estimated the costs of reprinting 3.5 million ballots statewide for a 2018 Democratic primary at $2 million, which adds up to $10 million for Democratic primaries over 10 years; the State spends $35-$45 million per ten years on paper ballots, and still has concerns over tampering.
* **Adaptability**.  An EVM must provide various voting rules and elections models, including proportional primaries, ranked ballots, and electoral fusion.

This software is built to power EVMs in an integrity model in which the EVM cannot be a black-box machine.  The software should run on hardware equivalent to a Raspberry Pi Zero or a Pi 3 B+ *without* the Wifi and Bluetooth chip, and so will be built to run on Alpine Linux and .NET Core.

Overall, a prototype EVM should cost around $150; production EVMs should cost $120.  Modern EVM makers sell their systems for upwards of $3,000; a voting system manufacturer should have an alternate revenue stream, such as public elections consulting and private parliamentary organization elections, and sell voting systems near cost.

# Voting Rules

For single-seat elections, this library provides the following vote count rules (recommended marked in bold):

* **Tideman's Alternative Smith**
* **Tideman's Alternative Schwartz**
* Ranked Pairs
* Schulze
* Instant Run-off Approval Voting
* Instant Run-off Voting
* Approval Voting
* Plurality

Schulze is suitable for parliamentary groups, as well, although the voting rule is mathematically-complex and requires less concern about voters understanding the voting rule itself.

Instant Run-off Voting tends to cause anomalies, such as in the 2009 Burlington Mayoral Election in which the Condorcet and Plurality winners both lost—raising the question of what exactly makes a candidate the IRV winner.

For multiple-seat elections, this library provides the following vote count rules:

* **Single Transferable Vote** (STV)
  * **Meek transfers**
  * **Droop Count**
* CPO-STV
* Schulze-STV
* Multiple Plurality

